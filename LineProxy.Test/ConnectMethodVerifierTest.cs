using System;
using ForwordProxy;
using Xunit;

namespace LineProxy.Test
{
    public class ConnectMethodVerifierTest
    {
        [Fact]
        public void IsConnectAndGetUrlTest()
        {
            var expectedUrl = "http://www.google.com";
            var request = $"CONNECT {expectedUrl} HTTP/1.1\r\n\r\n";

            Assert.True(ConnectMethodVerifier.IsConnectMethod(request));
            Assert.Equal(expectedUrl, ConnectMethodVerifier.GetUrlFromConnectMethod(request));
        }
    }
}