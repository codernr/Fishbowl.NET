namespace Fishbowl.Net.Client.Components.States
{
    public partial class Error
    {
        public string Message
        {
            get => this.message ?? "Unknown error";
            set => this.message = value;
        }

        private string? message;
    }
}