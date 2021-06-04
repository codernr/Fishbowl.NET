using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Client.Online.Services
{
    public interface IGameClient
    {
        Task Connected() => Task.CompletedTask;

        Task Reconnecting(Exception exception) => Task.CompletedTask;

        Task Reconnected(string connectionId) => Task.CompletedTask;

        Task Closed(Exception error) => Task.CompletedTask;

        Task ReceiveSetupPlayer(GameSetupViewModel setup) => Task.CompletedTask;

        Task ReceiveSetTeamName(TeamSetupViewModel teamSetup) => Task.CompletedTask;

        Task ReceiveWaitForTeamSetup(TeamSetupViewModel teamSetup) => Task.CompletedTask;

        Task ReceiveTeamName(TeamNameViewModel teamName) => Task.CompletedTask;

        Task ReceivePlayerCount(PlayerCountViewModel playerCount) => Task.CompletedTask;

        Task ReceiveWaitForOtherPlayers(PlayerViewModel player) => Task.CompletedTask;

        Task ReceiveRestoreState(PlayerViewModel player) => Task.CompletedTask;

        Task ReceiveGameAborted(GameAbortViewModel abort) => Task.CompletedTask;

        Task ReceiveGameStarted() => Task.CompletedTask;

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