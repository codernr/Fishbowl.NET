using System;

namespace Fishbowl.Net.Shared.Data
{
    public class GameContextJoin
    {
        public string Password { get; set; } = string.Empty;

        public Guid UserId { get; set; }
    }
    
    public class GameContextSetup
    {
        public GameContextJoin GameContextJoin { get; set; } = new();

        public GameSetup GameSetup { get; set; } = new();
    }
}