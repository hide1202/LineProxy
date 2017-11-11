using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LineProxy
{
    public static class ConnectMethodHandShake
    {
        private const string Connect = "CONNECT";
        private const string Http1_1 = "HTTP/1.1";

        public static bool IsConnectMethod(string requestString)
        {
            return requestString.Trim().StartsWith(Connect, StringComparison.CurrentCulture);
        }

        public static string GetUrlFromConnectMethod(string requestLine)
        {
            requestLine = requestLine.Trim();
            requestLine = requestLine.Replace(Connect, ""); // Remove CONNECT method
            var httpIndex = requestLine.IndexOf(Http1_1, StringComparison.Ordinal);
            requestLine =
                requestLine.Substring(0, httpIndex); // Remove HTTP/1.1 string
            return requestLine.Trim();
        }

        public static Task SendOk(TcpClient client)
        {
            var networkStream = client.GetStream();
            var ret = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK");
            return Awaits.RunIgnoreException(async () => { await networkStream.WriteAsync(ret, 0, ret.Length); });
        }
    }
}