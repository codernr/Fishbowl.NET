using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Common;
using MudBlazor;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class PlayerName
    {
        public Func<string, Task> OnPlayerNameSet { get; set; } = default!;

        private MudForm? form;

        private Once once = new();

        private string playerName = string.Empty;

        private Task Submit() =>
            this.once.Fire(() => this.OnPlayerNameSet(this.playerName));
    }
}