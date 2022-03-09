using System;
using System.Threading.Tasks;
using System.Timers;

namespace Interactivity.Entities
{
    internal sealed class TimeoutProvider
    {
        public double Delay { get; }

        private bool Disposed;
        private readonly Timer Timer;
        private readonly TaskCompletionSource<object> TimeoutSource;

        public TimeoutProvider(double delay)
        {
            Delay = delay;
            TimeoutSource = new TaskCompletionSource<object>();
            Timer = new Timer(delay)
            {
                AutoReset = false,
            };
            Timer.Elapsed += HandleTimerElapsed;
            Timer.Start();
        }
        public TimeoutProvider(TimeSpan delay)
            : this(delay.TotalMilliseconds)
        {
        }

        private void HandleTimerElapsed(object sender, ElapsedEventArgs e)
        {
            Disposed = true;
            Timer.Dispose();
            TimeoutSource.SetResult(null);
        }

        public void Reset()
        {
            if (Disposed)
            {
                return;
            }
            Timer.Stop();
            Timer.Start();
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            Disposed = true;
            Timer.Dispose();
            TimeoutSource.TrySetCanceled();
        }

        public Task WaitAsync()
            => TimeoutSource.Task;
    }
}
