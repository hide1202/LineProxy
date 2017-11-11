using System;
using System.Threading.Tasks;

namespace LineProxy
{
    public static class Awaits
    {
        public static async Task RunIgnoreException(Func<Task> action)
        {
            await Run(action, Console.WriteLine);
        }

        public static async Task Run(Func<Task> action, Action<Exception> onError)
        {
            try
            {
                await action?.Invoke();
            }
            catch (Exception exception)
            {
                onError?.Invoke(exception);
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
            var any = await Task.WhenAny(task, Task.Delay(timeout));
            return any != task;
        }
    }
}