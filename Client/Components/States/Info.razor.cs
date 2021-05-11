namespace Fishbowl.Net.Client.Components.States
{
    public partial class Info
    {
        public string Message
        {
            get => this.message ?? L?["Components.States.Info.UnknownError"] ?? "Unknown error";
            set => this.message = value;
        }

        private string? message;
    }
}