using System;

namespace Fishbowl.Net.Shared.Data
{
    public class Word
    {
        public Guid Id { get; init; }
        
        public string Value { get; init; }

        public Word(Guid id, string value) =>
            (this.Id, this.Value) = (id, value);
    }
}