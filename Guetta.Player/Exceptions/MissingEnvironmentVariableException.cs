using System;

namespace Guetta.Player.Exceptions
{
    internal sealed class MissingEnvironmentVariableException : Exception
    {
        public MissingEnvironmentVariableException(string variable) : base($"Environment variable '{variable}' was not found.")
        {
            Data.Add(nameof(variable), variable);
        }
    }
}