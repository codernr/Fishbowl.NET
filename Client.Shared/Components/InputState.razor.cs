using System;
using System.Threading.Tasks;

namespace Fishbowl.Net.Client.Shared.Components
{
    public abstract class InputState<T> : State<T> where T : InputState<T>
    {
        protected bool submitted = false;

        protected virtual Task Submit(Func<Task> submitAction)
        {
            if (this.submitted) return Task.CompletedTask;
            
            this.submitted = true;

            return submitAction();
        }

        protected virtual void Submit(Action submitAction)
        {
            if (this.submitted) return;
            
            this.submitted = true;

            submitAction();
        }
    }
}