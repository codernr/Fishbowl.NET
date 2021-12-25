using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class PeriodFinished
    {
        public PeriodSummaryViewModel Period => this.State.Value.Summary;
    }
}