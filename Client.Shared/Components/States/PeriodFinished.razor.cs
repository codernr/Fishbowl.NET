using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class PeriodFinished
    {
        public PeriodSummaryViewModel Period => this.State.Value.Period!;
    }
}