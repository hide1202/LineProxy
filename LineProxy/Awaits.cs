using System;
using System.Threading;
using System.Threading.Tasks;

namespace LineProxy
{
    public static class Awaits
    {
        public static async Task<bool> RunIgnoreException(Func<Task> action)
        {
            return await Run(action, Console.WriteLine);
        }

        public static async Task<bool> Run(Func<Task> action, Action<Exception> onError)
        {
            try
            {
                await action?.Invoke();
                return true;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                onError?.Invoke(exception);
                return false;
            }
        }

        public static async Task<T> Run<T>(Func<Task<T>> func, Func<Exception, T> onError)
        {
            try
            {
                return await (func != null ? func() : throw new ArgumentException(nameof(func)));
            }
            catch (Exception exception)
            {
                return onError != null ? onError(exception) : throw new ArgumentException(nameof(onError));
            }
        }

        public static async Task<bool> IsTimeout(Task task, TimeSpan timeout)
        {
            var cancel = new CancellationTokenSource();
            var any = await Task.WhenAny(task, Task.Delay(timeout, cancel.Token));
            if (any != task)
            {
                cancel.Cancel();
            }
            return any != task;
        }
    }
}