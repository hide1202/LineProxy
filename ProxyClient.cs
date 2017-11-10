using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ForwordProxy
{
    internal class ProxyClient
    {
        private TcpClient _client;
        private readonly TcpClient _toOriginClient;
        private readonly NetworkStream _toOriginRemoteStream;
        private readonly NetworkStream _toClientStream;

        public static ProxyClient NewClient(TcpClient client, string url)
        {
            return new ProxyClient(client, url);
        }

        private ProxyClient(TcpClient client, string url)
        {
            var remoteEndPoint = NetworkUtil.QueryDnsEntry(url).Result;
            Console.WriteLine("Remote Address : {0}", remoteEndPoint.Address);

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
            var bs = new byte[short.MaxValue];
            while (true)
            {
                try
                {
                    var read = await _toClientStream.ReadAsync(bs, 0, bs.Length);
                    var str = Encoding.Default.GetString(bs, 0, read);

                    var ret = Encoding.Default.GetString(bs, 0, read);
                    if (!string.IsNullOrEmpty(ret))
                    {
                        await WriteAsync(bs, 0, read);
                    }

                    ThreadUtil.SleepLoop();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        private async void ReceiveFromOrigin()
        {
            var bs = new byte[short.MaxValue];
            while (true)
            {
                if (!_toOriginClient.Connected)
                {
                    Console.WriteLine("Loss client connection");
                    return;
                }

                try
                {
                    var read = -1;
                    while ((read = await _toOriginRemoteStream.ReadAsync(bs, 0, bs.Length)) > 0)
                    {
                        await _toClientStream.WriteAsync(bs, 0, read);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

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
            }
        }
    }
}