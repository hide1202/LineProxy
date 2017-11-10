using System;

namespace ForwordProxy
{
    public static class ConnectMethodVerifier
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
    }
}