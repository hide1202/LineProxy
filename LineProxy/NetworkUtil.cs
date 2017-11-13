using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LineProxy
{
    internal static class NetworkUtil
    {
        internal static async Task<List<IPEndPoint>> QueryDnsEntry(string urlAndPort)
        {
            var domainAndPort = urlAndPort.Split(":");
            var hostEntryAsync = Dns.GetHostEntryAsync(domainAndPort[0]);

            return await Awaits.Run(async () =>
            {
                var isTimeout = await Awaits.IsTimeout(hostEntryAsync, TimeSpan.FromSeconds(10));
                if (isTimeout) return null;

                var entry = hostEntryAsync.Result;

                var port = int.Parse(domainAndPort[1]);
                return entry.AddressList.Select(addr =>
                {
                    Console.WriteLine($"Url [{urlAndPort}] IP Address{addr}");
                    return new IPEndPoint(addr, port);
                }).ToList();
            }, ex => null);
        }
    }
}