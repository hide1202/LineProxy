namespace LineProxy.Tracker
{
    public enum TrackType
    {
        ApplicationRun,
        Connect,
        FailToConnect,
        DisconnectClient,
        DisconnectOrigin,
        Receive,
        FailToReceive,
        Send,
        FailToSend
    }
}