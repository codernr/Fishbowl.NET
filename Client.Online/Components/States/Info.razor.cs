using Fishbowl.Net.Client.Online.Shared;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class Info
    {
        public string ContextClass { get; set; } = ContextCssClass.Primary;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool Loading { get; set; }
    }
}