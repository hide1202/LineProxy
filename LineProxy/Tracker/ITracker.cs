namespace LineProxy.Tracker
{
    public interface ITracker
    {
        void SendEvent(TrackType type);
    }
}