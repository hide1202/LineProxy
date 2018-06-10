using System;
using System.IO;
using System.Threading;
using LineProxy.Tracker;
using Microsoft.Extensions.Configuration;

namespace LineProxy
{
    public class Program
    {
        static void Main(string[] args)
        {
            AppSettings.Load();

            Logger.AzureTracker.SendEvent(TrackType.ApplicationRun);
            Console.WriteLine("Application : {0}",
                AppSettings.Current.ApplicationName);

            Console.WriteLine(AppSettings.Current.Azure);

            var listener = new GateListener(8081);

            try
            {
                listener.Start().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // Console.ReadKey();
            while(true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            listener.Stop();
        }
    }
}