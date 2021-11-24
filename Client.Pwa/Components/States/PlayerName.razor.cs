using System;
using System.Threading.Tasks;
using MudBlazor;

namespace Fishbowl.Net.Client.Pwa.Components.States
{
    public partial class PlayerName
    {
        public Func<string, Task> OnPlayerNameSet { get; set; } = default!;

        private MudForm? form;

        private string playerName = string.Empty;
    }
}