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
        PeriodSetupPlay,
        PeriodWatch,
        PeriodPlay,
        GameFinished
    }
}