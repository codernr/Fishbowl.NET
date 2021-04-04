using System;
using System.Collections.Generic;

namespace Fishbowl.Net.Shared.Data
{
    public class Player : IEquatable<Player>
    {
        public Guid Id { get; private set; }
        
        public string Name { get; private set; }
        
        public IEnumerable<Word> Words { get; init; }
        
        public Player(Guid id, string name, IEnumerable<Word> words) =>
            (this.Id, this.Name, this.Words) = (id, name, words);

        public bool Equals(Player? other) => other is not null && other.Id == this.Id;

        public override bool Equals(object? obj) => this.Equals(obj as Player);

        public override int GetHashCode() => this.Id.GetHashCode();

        public static bool operator ==(Player lhs, Player rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }
            
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Player lhs, Player rhs) => !(lhs == rhs);
    }
}