using System;

namespace Fishbowl.Net.Client.Shared.Services
{
    public class AppStateService
    {
        public event Action? PropertyChanged;

        public string Title
        {
            get => this.title;
            set
            {
                this.title = value;
                this.PropertyChanged?.Invoke();
            }
        }

        private string title = string.Empty;
    }
}