using System;

namespace Jellyfish.Commands
{
    internal class IllegalStateException : Exception
    {
        public IllegalStateException(string message) : base(message)
        {
        }
    }
}