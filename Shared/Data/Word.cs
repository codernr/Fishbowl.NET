using System;

namespace Fishbowl.Net.Shared.Data
{
    public class Word
    {
        public Guid Id { get; private set; }
        
        public string Value { get; private set; }

        public Word(Guid id, string value) =>
            (this.Id, this.Value) = (id, value);
    }
}