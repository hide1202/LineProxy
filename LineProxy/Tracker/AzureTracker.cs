using System.Diagnostics;
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

            Process.GetCurrentProcess().Exited += (sender, args) => _client?.Flush();
        }

        public void SendEvent(TrackType type)
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    _client.TrackEvent(type.ToString());
                    _client.Flush();
                });
            }
            catch
            {
                // ignored
            }
        }
    }
}