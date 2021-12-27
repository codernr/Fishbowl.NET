namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class PeriodWatch
    {
        protected override string Title => this.PeriodState.Value.Running.Round.Type;
    }
}