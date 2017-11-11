using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LineProxy
{
    internal static class NetworkUtil
    {
        internal static async Task<IPEndPoint> QueryDnsEntry(string urlAndPort)
        {
            var domainAndPort = urlAndPort.Split(":");
            var hostEntryAsync = Dns.GetHostEntryAsync(domainAndPort[0]);

            var timeoutSrc = new CancellationTokenSource();
            try
            {
                var task = await Task.WhenAny(hostEntryAsync, Task.Delay(TimeSpan.FromSeconds(3), timeoutSrc.Token));
                if (task == hostEntryAsync)
                {
                    timeoutSrc.Cancel();
                    var entry = hostEntryAsync.Result;
                    Console.WriteLine(entry.AddressList[0]);
                    return new IPEndPoint(entry.AddressList[0], int.Parse(domainAndPort[1]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }
    }
}