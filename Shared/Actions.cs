using System.Collections.Generic;
using Fishbowl.Net.Shared.GameEntities;
using Fishbowl.Net.Shared.ViewModels;

namespace Fishbowl.Net.Shared.Actions
{
    public record ReceivePlayerCountAction(int TotalCount, int ConnectedCount, int SetupCount);

    public record ReceiveRestoreStateAction(PlayerViewModel Player, List<TeamViewModel> Teams);

    public record ReceiveWaitForTeamSetupAction(PlayerViewModel? SetupPlayer, List<TeamViewModel> Teams);
    
    public record ReceiveGameAbortAction(string MessageKey);

    public record ReceiveTeamNameAction(int Id, string Name);

    public record JoinGameContextAction(string Password, string Username);

    public record CreateGameContextAction(JoinGameContextAction GameContextJoin, GameSetupViewModel GameSetup);

    public record SubmitTeamNameAction(int Id, string Name);
    
    public record AddPlayerAction(string Username, IEnumerable<Word> Words);

    public record AddScoreAction(WordViewModel Word);

    public static class ActionExtensions
    {
        public static Player Map(this AddPlayerAction player) =>
            new(player.Username, player.Words);

    }
}