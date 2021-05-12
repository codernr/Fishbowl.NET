using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;
using Fishbowl.Net.Shared.Data.ViewModels;

namespace Fishbowl.Net.Client.Services
{
    public interface IGameClient
    {
        Task ReceiveSetupPlayer(GameSetup setup) => Task.CompletedTask;

        Task ReceivePlayerCount(PlayerCountViewModel playerCount) => Task.CompletedTask;

        Task ReceiveWaitForOtherPlayers(Player player) => Task.CompletedTask;

        Task RestoreGameState(Player player, Round round) => Task.CompletedTask;

        Task ReceiveGameAborted(string message) => Task.CompletedTask;

        Task ReceiveGameStarted(IEnumerable<TeamViewModel> teams) => Task.CompletedTask;

        Task ReceiveGameFinished(Game game) => Task.CompletedTask;

        Task ReceiveRoundStarted(Round round) => Task.CompletedTask;

        Task ReceiveRoundFinished(Round round) => Task.CompletedTask;

        Task ReceivePeriodSetup(Period period) => Task.CompletedTask;

        Task ReceivePeriodStarted(Period period) => Task.CompletedTask;

        Task ReceivePeriodFinished(Period period) => Task.CompletedTask;

        Task ReceiveWordSetup(Word word) => Task.CompletedTask;

        Task ReceiveScoreAdded(Score score) => Task.CompletedTask;

        Task ReceiveLastScoreRevoked(Score score) => Task.CompletedTask;
    }
}