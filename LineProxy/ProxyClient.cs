using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LineProxy.Tracker;

namespace LineProxy
{
    internal class ProxyClient
    {
        private TcpClient _client;
        private readonly TcpClient _toOriginClient;
        private readonly NetworkStream _toOriginRemoteStream;
        private readonly NetworkStream _toClientStream;

        public static ProxyClient NewClient(TcpClient client, IPEndPoint remoteEndPoint)
        {
            return new ProxyClient(client, remoteEndPoint);
        }

        private ProxyClient(TcpClient client, IPEndPoint remoteEndPoint)
        {
            _toOriginClient = new TcpClient();
            _toOriginClient.Connect(remoteEndPoint.Address, remoteEndPoint.Port);

            _client = client;
            _toClientStream = client.GetStream();
            _toOriginRemoteStream = _toOriginClient.GetStream();

            Task.Factory.StartNew(ReceiveFromOrigin);
            Task.Factory.StartNew(ReceiveFromClient);
        }

        private async void ReceiveFromClient()
        {
            var failCount = 0;

            var bs = new byte[short.MaxValue];
            while (true)
            {
                var isSuccess = await Awaits.Run(async () =>
                {
                    if (!_client.Connected)
                    {
                        Console.WriteLine("Loss client connection");
                        Logger.AzureTracker.SendEvent(TrackType.DisconnectClient);
                        return false;
                    }

                    var read = await _toClientStream.ReadAsync(bs, 0, bs.Length);

                    var ret = Encoding.Default.GetString(bs, 0, read);
                    if (!string.IsNullOrEmpty(ret))
                    {
                        await WriteAsync(bs, 0, read);
                    }

                    failCount = 0;
                    ThreadUtil.SleepLoop();
                    return true;
                }, ex =>
                {
                    failCount++;
                    Console.WriteLine(ex);
                    Logger.AzureTracker.SendEvent(TrackType.FailToReceive);

                    if (failCount > 5 && !_client.Connected)
                    {
                        Console.WriteLine("Disconnect client!");
                        Logger.AzureTracker.SendEvent(TrackType.DisconnectClient);
                    }
                    return false;
                });

                if (!isSuccess)
                    break;
            }
        }

        private async void ReceiveFromOrigin()
        {
            var failCount = 0;

            var bs = new byte[short.MaxValue];
            while (true)
            {
                if (!_toOriginClient.Connected || !_client.Connected)
                {
                    Console.WriteLine($"Loss client connection, " +
                                      $"toOrigin Connected[{_toOriginClient.Connected}] client Connected[{_client.Connected}]");
                    return;
                }

                var isSuccess = await Awaits.Run(async () =>
                {
                    var sendBytes = 0;

                    var read = -1;
                    while ((read = await _toOriginRemoteStream.ReadAsync(bs, 0, bs.Length)) > 0)
                    {
                        await _toClientStream.WriteAsync(bs, 0, read);
                        sendBytes += read;
                    }
                    failCount = 0;
                }, ex =>
                {
                    failCount++;
                    Console.WriteLine(ex);

                    if (failCount > 5 && !_client.Connected)
                    {
                        Console.WriteLine("Disconnect origin server!");
                        Logger.AzureTracker.SendEvent(TrackType.DisconnectOrigin);
                    }
                });

                if (!isSuccess)
                    break;

                ThreadUtil.SleepLoop();
            }
        }

        public async Task WriteAsync(byte[] buffer, int offset, int length)
        {
            try
            {
                await _toOriginRemoteStream.WriteAsync(buffer, offset, length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Logger.AzureTracker.SendEvent(TrackType.FailToSend);
            }
        }
    }
}