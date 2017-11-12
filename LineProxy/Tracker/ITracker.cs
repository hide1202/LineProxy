using System.Collections.Generic;

namespace LineProxy.Tracker
{
    public interface ITracker
    {
        void SendEvent(TrackType type);

        void SendEventWithMetrics(TrackType type, params (MetricType k, double v)[] metricParams);
    }
}