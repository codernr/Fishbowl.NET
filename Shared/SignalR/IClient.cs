using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared.SignalR
{
    public interface IClient
    {
        Task DefineTeamCount();

        Task DefineRoundTypes();

        Task ReceiveTeams(IEnumerable<Team> teams);

        Task ReceiveRound(Round round);

        Task ReceivePeriod(Period period);

        Task ReceiveWord(Word word);

        Task ReceiveScore(Score score);

        Task ReceiveGame(Game game);
    }
}