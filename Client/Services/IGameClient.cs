using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Services
{
    public interface IGameClient
    {
        Task ReceiveSetupPlayer(GameSetup setup);

        Task ReceiveWaitForOtherPlayers(Player player);

        Task ReceiveGameState(Game game);

        Task ReceiveGameAborted(string message);

        Task ReceiveGameStarted(Game game);

        Task ReceiveGameFinished(Game game);

        Task ReceiveRoundStarted(Round round);

        Task ReceiveRoundFinished(Round round);

        Task ReceivePeriodSetup(Period period);

        Task ReceivePeriodStarted(Period period);

        Task ReceivePeriodFinished(Period period);

        Task ReceiveWordSetup(Word word);

        Task ReceiveScoreAdded(Score score);
    }
}