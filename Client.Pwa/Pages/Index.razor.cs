using System;
using System.Threading.Tasks;
using Fishbowl.Net.Client.Shared.Components;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Pwa.Pages
{
    public partial class Index
    {
        private StateManager? stateManager;

        private StateManager StateManager => this.stateManager ?? throw new InvalidOperationException();

        private string L(string key) => this.StringLocalizer[key] ?? key;

        private Task SetPlayerCount(int playerCount) => Task.CompletedTask;

        private Task SetTeamCount(int teamCount) => Task.CompletedTask;

        private Task SetWordCount(int wordCount) => Task.CompletedTask;

        private Task SetRoundTypes(string[] roundTypes) => Task.CompletedTask;

        private Task SetPlayerName(string name) => Task.CompletedTask;

        private Task SetPlayerData(string[] words) => Task.CompletedTask;

        private Task SetTeamName(TeamNameViewModel teamName) => Task.CompletedTask;

        private Task StartPeriod(DateTimeOffset timestamp) => Task.CompletedTask;

        private Task AddScore(ScoreViewModel score) => Task.CompletedTask;

        private Task FinishPeriod(DateTimeOffset timestamp) => Task.CompletedTask;

        private Task RevokeLastScore() => Task.CompletedTask;

        private void Reload() => this.NavigationManager.NavigateTo(this.NavigationManager.Uri, true);
    }
}