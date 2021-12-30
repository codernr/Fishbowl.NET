namespace Fishbowl.Net.Client.Online.Components.Screens
{
    public partial class PeriodSetupWatch
    {
        protected override string Title => this.State.Value.Current.Setup.Round.Type;
    }

}