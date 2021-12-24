namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class Info
    {
        protected override string Title => this.State.Value.Title ?? base.Title;
    }
}