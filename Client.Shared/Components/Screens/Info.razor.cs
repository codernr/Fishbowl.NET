namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class Info
    {
        protected override string Title => this.State.Value.Title ?? base.Title;
    }
}