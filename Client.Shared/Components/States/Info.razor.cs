using MudBlazor;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class Info
    {
        public Severity Severity { get; set; }

        public string? Message { get; set; }

        public bool Loading { get; set; }
    }
}