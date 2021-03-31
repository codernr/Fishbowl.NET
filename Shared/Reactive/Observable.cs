using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Fishbowl.Net.Shared.Reactive
{
    public class Observable<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> source;

        private readonly List<Task<T>> values = new();

        private readonly TaskCompletionSource start = new();

        private Task? readTask;

        private Task ReadTask
        {
            get
            {
                if (this.readTask is null)
                {
                    this.readTask = this.ReadSource();
                }

                return this.readTask;
            }
        }

        public Observable(IAsyncEnumerable<T> source) => this.source = source;

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new ObservableEnumerator<T>(this.values, this.ReadTask);

        private async Task ReadSource()
        {
            TaskCompletionSource<T> previous = new();
            TaskCompletionSource<T> next;
            
            this.values.Add(previous.Task);
            await foreach (var item in this.source)
            {
                next = new();
                this.values.Add(next.Task);
                previous.SetResult(item);
                previous = next;
            }
        }
    }
}