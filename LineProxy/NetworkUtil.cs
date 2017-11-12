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

            return await Awaits.Run(async () =>
            {
                var isTimeout = await Awaits.IsTimeout(hostEntryAsync, TimeSpan.FromSeconds(3));
                if (isTimeout) return null;

                var entry = hostEntryAsync.Result;
                Console.WriteLine(entry.AddressList[0]);
                return new IPEndPoint(entry.AddressList[0], int.Parse(domainAndPort[1]));
            }, ex => null);
        }
    }
}