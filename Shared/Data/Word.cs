using System;

namespace Fishbowl.Net.Shared.Data
{
    public class Word
    {
        public Guid Id { get; private set; }
        
        public string Value { get; private set; }

        public Word(Guid id, string value) =>
            (this.Id, this.Value) = (id, value);

        public bool Equals(Word? other) => other is not null && other.Id == this.Id;

        public override bool Equals(object? obj) => this.Equals(obj as Word);

        public override int GetHashCode() => this.Id.GetHashCode();

        public static bool operator ==(Word lhs, Word rhs)
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

        public static bool operator !=(Word lhs, Word rhs) => !(lhs == rhs);
    }
}