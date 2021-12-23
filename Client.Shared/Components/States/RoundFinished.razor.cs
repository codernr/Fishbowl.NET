using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class RoundFinished
    {
        public RoundSummaryViewModel Round => this.State.Value.Round!;
    }
}