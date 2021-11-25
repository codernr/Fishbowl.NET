using System;
using System.Collections.Generic;
using System.Linq;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.States
{
    public partial class GameFinished
    {
        public Action ReloadRequested { get; set; } = default!;
        
        public GameSummaryViewModel Game { get; set; } = default!;

        private Dictionary<string, int> teamScores = default!;

        private string[] winnerTeams = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.teamScores = this.Game.Teams
                .ToDictionary(
                    team => team.Name,
                    team => team.Players.Sum(player => player.Scores.Count));

            var max = this.teamScores.Max(team => team.Value);

            this.winnerTeams = this.teamScores
                .Where(team => team.Value == max)
                .Select(team => team.Key)
                .ToArray();
        }
    }
}