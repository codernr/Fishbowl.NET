namespace Fishbowl.Net.Client.Shared
{
    public enum GameState
    {
        Connecting,
        WaitingForTeamCount,
        WaitingForRoundTypes,
        WaitingForPlayer,
        WaitingForPlayers,
        GameStarted,
        RoundStarted,
        PeriodSetupWatch,
        PeriodSetupPlay,
        PeriodWatch,
        PeriodPlay,
        PeriodFinished,
        RoundFinished,
        GameFinished
    }
}