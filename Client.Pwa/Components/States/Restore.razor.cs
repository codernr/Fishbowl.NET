using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class Restore
    {
        public Func<Task> OnRestoreRequested { get; set; } = default!;

        public Func<Task> OnNewGameRequested { get; set; } = default!;

        private Once once = new();

        private Task RequestRestore() => this.once.Fire(this.OnRestoreRequested);

        private Task RequestNewGame() => this.once.Fire(this.OnNewGameRequested);
    }
}