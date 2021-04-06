using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Client.Services
{
    public interface IGameClient
    {
        Task DefineTeamCount();

        Task DefineRoundTypes();

        Task DefinePlayer();

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