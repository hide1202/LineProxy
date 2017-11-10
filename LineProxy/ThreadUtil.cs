using System;
using System.Threading;

namespace ForwordProxy
{
    public class ThreadUtil
    {
        public static readonly TimeSpan LoopSleepTime = TimeSpan.FromMilliseconds(10);

        public static void SleepLoop()
        {
            Thread.Sleep(LoopSleepTime);
        }
    }
}