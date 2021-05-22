using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Client.Shared;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Services
{
    public interface IClientState
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string? Password { get; set; }

        bool IsCreating { get; set; }

        int TotalPlayerCount { get; set; }

        int SetupPlayerCount { get; set; }

        int ConnectedPlayerCount { get; set; }

        int WordCount { get; set; }

        int TeamCount { get; set; }
        
        string[] RoundTypes { get; set; }

        List<TeamViewModel> Teams { get; set; }

        TeamViewModel Team => this.Teams.First(team =>
            team.Players.Any(player => player.Id == this.Id));

        List<ScoreViewModel> PeriodScores { get; }
    }

    public class ClientState : IClientState
    {
        public Guid Id { get => this.id.Value; set => this.id.Value = value; }

        private readonly PersistedProperty<Guid> id;

        public string Name { get => this.name.Value; set => this.name.Value = value; }

        private readonly PersistedProperty<string> name;

        public string? Password
        {
            get => this.Expires < DateTime.Now ? null : this.password.Value;
            set
            {
                this.password.Value = value;
                this.Expires = DateTime.Now + ExpiresProperty.DefaultExpiryTime;
            }
        }

        private readonly PersistedProperty<string?> password;

        public bool IsCreating { get; set; }

        public int TotalPlayerCount { get; set; } = 4;

        public int SetupPlayerCount { get; set; } = 0;

        public int ConnectedPlayerCount { get; set; } = 0;
        
        public int WordCount { get; set; } = 2;

        public int TeamCount { get; set; } = 2;
        
        public string[] RoundTypes { get; set; } = new string[0];

        public List<TeamViewModel> Teams { get; set; } = new();

        public List<ScoreViewModel> PeriodScores { get; } = new();

        private DateTime Expires { get => this.expires.Value; set => this.expires.Value = value; }

        private readonly PersistedProperty<DateTime> expires;

        public ClientState(IStorageService storageService)
        {
            this.id = new IdProperty(storageService);
            this.name = new NameProperty(storageService);
            this.password = new PasswordProperty(storageService);
            this.expires = new ExpiresProperty(storageService);
        }
    }
}