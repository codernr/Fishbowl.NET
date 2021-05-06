namespace Fishbowl.Net.Client.Components.States
{
    public partial class Error
    {
        public string Message
        {
            get => this.message ?? L?["Components.States.Error.UnknownError"] ?? "Unknown error";
            set => this.message = value;
        }

        private string? message;
    }
}