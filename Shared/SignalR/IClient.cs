using System.Collections.Generic;
using System.Threading.Tasks;
using Fishbowl.Net.Shared.Data;

namespace Fishbowl.Net.Shared.SignalR
{
    public interface IClient
    {
        Task DefineTeamCount();

        Task DefineRoundTypes();

        Task ReceiveTeams(IEnumerable<TeamViewModel> teams);

        Task ReceiveRound(string roundType);

        Task ReceivePeriod(Player player);

        Task ReceiveWord(Word word);

        Task ReceiveScore(Score score);

        Task ReceiveResults(Dictionary<int, int> scores);
    }
}