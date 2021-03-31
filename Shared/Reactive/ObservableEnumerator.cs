using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fishbowl.Net.Shared.Reactive
{
    public class ObservableEnumerator<T> : IAsyncEnumerator<T>
    {
        public T Current => this.list[this.id].Result;

        private int id = -1;

        private readonly List<Task<T>> list;

        private readonly Task completion;

        public ObservableEnumerator(List<Task<T>> list, Task completion) =>
            (this.list, this.completion) = (list, completion);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public async ValueTask<bool> MoveNextAsync()
        {
            if (this.id < this.list.Count - 1)
            {
                this.id++;
                await Task.WhenAny(this.list[this.id], this.completion);

                if (this.completion.IsCompletedSuccessfully)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}