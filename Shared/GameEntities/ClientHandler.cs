using System.Threading.Tasks;

namespace Fishbowl.Net.Shared.GameEntities
{
    public interface IClientHandler
    {
        Task OnGameCreated(Game game) => Task.CompletedTask;
        Task OnPlayerSetup(Game game) => Task.CompletedTask;
        Task OnGameStarted(Game game) => Task.CompletedTask;
        Task OnGameFinished(Game game) => Task.CompletedTask;
        Task OnRoundStarted(Game game) => Task.CompletedTask;
        Task OnRoundFinished(Game game) => Task.CompletedTask;
        Task OnPeriodSetup(Game game) => Task.CompletedTask;
        Task OnPeriodStarted(Game game) => Task.CompletedTask;
        Task OnPeriodFinished(Game game) => Task.CompletedTask;
        Task OnScoreAdded(Game game) => Task.CompletedTask;
        Task OnLastScoreRevoked(Game game) => Task.CompletedTask;
        Task OnWordSetup(Game game) => Task.CompletedTask;
        Task OnTimerUpdate(Game game) => Task.CompletedTask;
    }
}