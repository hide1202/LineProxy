using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LineProxy.Tracker;

namespace LineProxy
{
    public class GateListener
    {
        private readonly TcpListener _listener;

        private readonly ConcurrentDictionary<EndPoint, TcpClient> _acceptClients
            = new ConcurrentDictionary<EndPoint, TcpClient>();

        private readonly ConcurrentDictionary<EndPoint, ProxyClient> _proxies
            = new ConcurrentDictionary<EndPoint, ProxyClient>();

        public GateListener(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public Task Start()
        {
            _listener.Start();

            return Task.Factory.StartNew(async () =>
            {
                var bs = new byte[short.MaxValue];
                while (true)
                {
                    var isSuccess = await Awaits.RunIgnoreException(async () =>
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        var clientEndPoint = client.Client.RemoteEndPoint;
                        if (!_acceptClients.TryAdd(clientEndPoint, client))
                        {
                            var clientIp = clientEndPoint as IPEndPoint;
                            Console.WriteLine("Already has client : {0}", clientIp?.Address);
                            return;
                        }
                        Console.WriteLine("Accept client!");

                        var stream = client.GetStream();

                        var str = await stream.ReadStringAsync(bs);
                        if (ConnectMethodHandShake.IsConnectMethod(str))
                        {
                            var url = ConnectMethodHandShake.GetUrlFromConnectMethod(str);

                            var canConnect = await CanConnect(url);
                            if (canConnect.isSuccess)
                            {
                                Console.WriteLine("Success to connect!");
                                Logger.AzureTracker.SendEvent(TrackType.Connect);

                                await ConnectMethodHandShake.SendOk(client);

                                var proxy = ProxyClient.NewClient(client, canConnect.ip);

                                _proxies.AddOrUpdate(clientEndPoint, proxy,
                                    (point, proxyClient) => proxy);
                            }
                            else
                            {
                                Console.WriteLine("Fail to connect!");
                                Logger.AzureTracker.SendEvent(TrackType.FailToConnect);
                            }
                        }
                    });
                    ThreadUtil.SleepLoop();

                    if (!isSuccess)
                    {
                        Console.WriteLine("Raise exception when accepting clients");
                        break;
                    }
                }
            });
        }

        public void Stop()
        {
            foreach (var kv in _acceptClients)
            {
                kv.Value?.Close();
            }
            _listener.Stop();
        }

        private async Task<(bool isSuccess, IPEndPoint ip)> CanConnect(string url)
        {
            return await Awaits.Run(async () =>
            {
                var remoteEndPoints = await NetworkUtil.QueryDnsEntry(url);
                if (remoteEndPoints == null)
                    return (false, null);

                using (var tryConnectClient = new TcpClient())
                {
                    foreach (var ep in remoteEndPoints)
                    {
                        var connectTask = tryConnectClient.ConnectAsync(ep.Address, ep.Port);

                        var isTimeout = await Awaits.IsTimeout(connectTask, TimeSpan.FromSeconds(10));
                        if (!isTimeout)
                            return (true, ep);
                    }

                    return (false, null);
                }
            }, ex =>
            {
                Console.WriteLine(ex);
                return (false, null);
            });
        }
    }
}