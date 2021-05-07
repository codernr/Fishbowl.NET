namespace Fishbowl.Net.Shared.Data
{
    public class GameSetup
    {
        public int PlayerCount { get; set; } = 4;
        
        public int WordCount { get; set; } = 2;

        public int TeamCount { get; set; } = 2;
        
        public string[] RoundTypes { get; set; } =
            new[] { "Taboo", "Charades", "Password", "Humming" };
    }
}