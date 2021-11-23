using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class InputState
    {
        protected bool submitted = false;

        protected virtual Task Submit(Func<Task> submitAction)
        {
            if (this.submitted) return Task.CompletedTask;
            
            this.submitted = true;

            return submitAction();
        }
    }
}