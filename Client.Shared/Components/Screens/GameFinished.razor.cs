using System.Linq;
using Fishbowl.Net.Client.Shared.Store;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Shared.Components.Screens
{
    public partial class GameFinished
    {
        private GameSummaryViewModel Game => this.State.Value.Summary;

        private PlayerSummaryViewModel bestPlayer = default!;

        private WordViewModel[] wordRank = default!;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.bestPlayer = this.Game.Teams
                .SelectMany(team => team.Players)
                .OrderByDescending(player => player.Scores.Count)
                .ThenBy(player => player.Scores.Sum(score => score.GuessedTime.Ticks))
                .First();

            this.wordRank = this.Game.Teams
                .SelectMany(team => team.Players)
                .SelectMany(player => player.Scores)
                .GroupBy(score => score.Word.Id)
                .OrderByDescending(g => g.Average(score => score.GuessedTime.Ticks))
                .Select(group => group.First().Word)
                .ToArray();
        }

        private void Reload() => this.Dispatcher.Dispatch(new ReloadAction());
    }
}