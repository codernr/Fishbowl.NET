using System;

namespace Fishbowl.Net.Client.Shared
{
    public record ClientState()
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Name { get; init; } = string.Empty;

        public DateTime Expires { get; init; } = DateTime.Now + TimeSpan.FromMinutes(30);

        public bool IsCreating { get; set; } = false;

        public string? Password
        {
            get => DateTime.Now < this.Expires ? this.password : null;
            init => this.password = value;
        }

        private string? password;
    }
}