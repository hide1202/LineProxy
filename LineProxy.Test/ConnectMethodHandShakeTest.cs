using System;
using Xunit;

namespace LineProxy.Test
{
    public class ConnectMethodHandShakeTest
    {
        [Fact]
        public void IsConnectAndGetUrlTest()
        {
            var expectedUrl = "http://www.google.com";
            var request = $"CONNECT {expectedUrl} HTTP/1.1\r\n\r\n";

            Assert.True(ConnectMethodHandShake.IsConnectMethod(request));
            Assert.Equal(expectedUrl, ConnectMethodHandShake.GetUrlFromConnectMethod(request));
        }
    }
}