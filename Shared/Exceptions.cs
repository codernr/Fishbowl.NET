using System;

namespace Fishbowl.Net.Shared.Exceptions
{
    public class InvalidReturnValueException : ArgumentException
    {
        public InvalidReturnValueException() : base() {}
        public InvalidReturnValueException(string message) : base(message) {}
        public InvalidReturnValueException(string message, Exception inner) : base(message, inner) {}
    }
}