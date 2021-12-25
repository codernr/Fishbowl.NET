using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class PeriodWatch
    {
        public PeriodRunningViewModel Period => this.State.Value.Period;
    }
}