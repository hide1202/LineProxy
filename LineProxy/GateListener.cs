using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForwordProxy
{
    public class GateListener
    {
        private TcpListener _listener;

        private readonly Dictionary<EndPoint, TcpClient> _acceptClients
            = new Dictionary<EndPoint, TcpClient>();

        private readonly Dictionary<EndPoint, ProxyClient> _proxies
            = new Dictionary<EndPoint, ProxyClient>();

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
                    try
                    {
                        var client = await _listener.AcceptTcpClientAsync();
                        _acceptClients.Add(client.Client.RemoteEndPoint, client);
                        var stream = client.GetStream();

                        Console.WriteLine("Accept client!");

                        var read = await stream.ReadAsync(bs, 0, bs.Length);
                        var str = Encoding.Default.GetString(bs, 0, read);
                        if (ConnectMethodVerifier.IsConnectMethod(str))
                        {
                            var url = ConnectMethodVerifier.GetUrlFromConnectMethod(str);

                            var isConnect = await CanConnect(url);
                            if (isConnect)
                            {
                                Console.WriteLine("Success to connect!");

                                var ret = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK");
                                await stream.WriteAsync(ret, 0, ret.Length);
                                var proxy = ProxyClient.NewClient(client, url);
                                if (_proxies.ContainsKey(client.Client.RemoteEndPoint))
                                {
                                    _proxies.Remove(client.Client.RemoteEndPoint);
                                }
                                _proxies.Add(client.Client.RemoteEndPoint, proxy);
                            }
                            else
                            {
                                Console.WriteLine("Fail to connect!");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    ThreadUtil.SleepLoop();
                }
            });
        }

        public void Stop()
        {
            _listener.Stop();
            foreach (var kv in _acceptClients)
            {
                kv.Value?.Close();
            }
        }

        private async Task<bool> CanConnect(string url)
        {
            try
            {
                var remoteEndPoint = await NetworkUtil.QueryDnsEntry(url);
                if (remoteEndPoint == null)
                    return false;

                Console.WriteLine($"Query dns : {remoteEndPoint}");

                var tryConnectClient = new TcpClient
                {
                    SendTimeout = 3000,
                    ReceiveTimeout = 3000
                };

                var connect = tryConnectClient.ConnectAsync(remoteEndPoint.Address, remoteEndPoint.Port);
                var task = await Task.WhenAny(connect, Task.Delay(TimeSpan.FromSeconds(3)));
                if (task == connect)
                {
                    return tryConnectClient.Connected;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}