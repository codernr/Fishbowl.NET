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

        Task ReceiveGameAborted(GameAbortViewModel abort) => Task.CompletedTask;

        Task ReceiveGameStarted(GameViewModel game) => Task.CompletedTask;

        Task ReceiveGameFinished(GameSummaryViewModel game) => Task.CompletedTask;

        Task ReceiveRoundStarted(RoundViewModel round) => Task.CompletedTask;

        Task ReceiveRoundFinished(RoundSummaryViewModel round) => Task.CompletedTask;

        Task ReceivePeriodSetup(PeriodSetupViewModel period) => Task.CompletedTask;

        Task ReceivePeriodStarted(PeriodRunningViewModel period) => Task.CompletedTask;

        Task ReceivePeriodFinished(PeriodSummaryViewModel period) => Task.CompletedTask;

        Task ReceiveWordSetup(WordViewModel word) => Task.CompletedTask;

        Task ReceiveScoreAdded(ScoreViewModel score) => Task.CompletedTask;

        Task ReceiveLastScoreRevoked(ScoreViewModel score) => Task.CompletedTask;
    }
}