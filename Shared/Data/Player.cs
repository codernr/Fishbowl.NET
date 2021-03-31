using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Player
    {
        public Guid Id { get; init; }
        
        public string Name { get; init; }
        
        public IEnumerable<Word> Words { get; init; }
        
        public Player(Guid id, string name, IEnumerable<Word> words) =>
            (this.Id, this.Name, this.Words) = (id, name, words);
    }
}