using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Common
{
    public class Once
    {
        public bool Fired { get; private set; } = false;

        public Task Fire(Func<Task> action)
        {
            if (this.Fired) return Task.CompletedTask;

            this.Fired = true;

            return action();
        }
    }
}