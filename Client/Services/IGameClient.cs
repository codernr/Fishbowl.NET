using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Services
{
    public interface IGameClient
    {
        Task ReceiveSetupPlayer(GameSetup setup);

        Task ReceiveConnectionCount(int connectionCount);

        Task ReceiveWaitForOtherPlayers(Player player);

        Task RestoreGameState(Player player, Round round);

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

        Task ReceiveLastScoreRevoked(Score score);
    }
}