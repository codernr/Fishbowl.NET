using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fishbowl.Net.Shared.Reactive
{
    public class Observable<T>
    {
        private readonly IAsyncEnumerable<T> source;

        private readonly List<Task<T>> values = new List<Task<T>>();

        private Task? readTask;

        public Observable(IAsyncEnumerable<T> source) => this.source = source;

        public async IAsyncEnumerable<T> GetStream()
        {
            if (this.readTask is null)
            {
                this.readTask = this.ReadSource();
            }

            for (int i = 0; i < this.values.Count; i++)
            {
                await Task.WhenAny(this.values[i], this.readTask);
                
                if (this.readTask.IsCompletedSuccessfully)
                {
                    yield break;
                }

                yield return await this.values[i];
            }
        }

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