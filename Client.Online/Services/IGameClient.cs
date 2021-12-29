using System;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Actions;

namespace Fishbowl.Net.Client.Online.Services
{
    public interface IGameClient
    {
        Task Connected() => Task.CompletedTask;

        Task Reconnecting(Exception? exception) => Task.CompletedTask;

        Task Reconnected(string? connectionId) => Task.CompletedTask;

        Task Closed(Exception? error) => Task.CompletedTask;

        Task ReceiveGameSetup(ReceiveGameSetupAction action) => Task.CompletedTask;

        Task ReceiveSetTeamName(ReceiveSetTeamNameAction action) => Task.CompletedTask;

        Task ReceiveWaitForTeamSetup(ReceiveWaitForTeamSetupAction action) => Task.CompletedTask;

        Task ReceiveTeamName(ReceiveTeamNameAction action) => Task.CompletedTask;

        Task ReceivePlayerCount(ReceivePlayerCountAction action) => Task.CompletedTask;

        Task ReceiveWaitForOtherPlayers(ReceiveWaitForOtherPlayersAction action) => Task.CompletedTask;

        Task ReceiveRestoreState(ReceiveRestoreStateAction action) => Task.CompletedTask;

        Task ReceiveGameAborted(ReceiveGameAbortAction action) => Task.CompletedTask;

        Task ReceiveGameStarted(ReceiveGameStartedAction action) => Task.CompletedTask;

        Task ReceiveGameFinished(ReceiveGameFinishedAction action) => Task.CompletedTask;

        Task ReceiveRoundStarted(ReceiveRoundStartedAction action) => Task.CompletedTask;

        Task ReceiveRoundFinished(ReceiveRoundFinishedAction action) => Task.CompletedTask;

        Task ReceivePeriodSetup(ReceivePeriodSetupAction action) => Task.CompletedTask;

        Task ReceivePeriodStarted(ReceivePeriodStartedAction action) => Task.CompletedTask;

        Task ReceivePeriodFinished(ReceivePeriodFinishedAction action) => Task.CompletedTask;

        Task ReceiveWordSetup(ReceiveWordSetupAction action) => Task.CompletedTask;

        Task ReceiveScoreAdded(ReceiveScoreAddedAction action) => Task.CompletedTask;

        Task ReceiveLastScoreRevoked(ReceiveLastScoreRevokedAction action) => Task.CompletedTask;

        Task ReceiveTimerUpdate(ReceiveTimerUpdateAction action) => Task.CompletedTask;
    }
}