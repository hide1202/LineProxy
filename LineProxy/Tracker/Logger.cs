namespace LineProxy.Tracker
{
    public static class Logger
    {
        public static ITracker AzureTracker { get; } = new AzureTracker();
    }
}