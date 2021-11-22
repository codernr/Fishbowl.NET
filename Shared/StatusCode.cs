namespace Fishbowl.Net.Shared
{
    public enum StatusCode
    {
        Ok,
        ConnectionAlreadyAssigned,
        GameContextExists,
        GameContextNotFound,
        GameContextFull,
        ConcurrencyError,
        UsernameTaken
    }
}