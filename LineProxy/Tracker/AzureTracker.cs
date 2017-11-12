using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;

namespace LineProxy.Tracker
{
    public class AzureTracker : ITracker
    {
        private readonly TelemetryClient _client;

        public AzureTracker()
        {
            _client = new TelemetryClient
            {
                InstrumentationKey = AppSettings.Current.Azure.InstrumentationKey
            };
            _client.Context.User.Id = Environment.UserName;
            _client.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

            Process.GetCurrentProcess().Exited += (sender, args) => _client?.Flush();
        }

        public void SendEvent(TrackType type)
        {
            Task.Factory.StartNew(() =>
            {
                _client.TrackEvent(type.ToString());
                _client.Flush();
            });
        }

        public void SendEventWithMetrics(TrackType type, params (MetricType k, double v)[] metricParams)
        {
            Task.Factory.StartNew(() =>
            {
                var metrics = metricParams.ToDictionary(m => m.k.ToString(), m => m.v);
                _client.TrackEvent(type.ToString(), metrics: metrics);
                _client.Flush();
            });
        }
    }
}