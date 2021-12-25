using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Components.States
{
    public partial class PeriodWatch
    {
        public PeriodRunningViewModel Period => this.State.Value.Period;
    }
}