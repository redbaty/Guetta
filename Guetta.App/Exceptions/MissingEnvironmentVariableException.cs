using System;

namespace Guetta.App.Exceptions
{
    public sealed class MissingEnvironmentVariableException : Exception
    {
        public MissingEnvironmentVariableException(string variable) : base($"Environment variable '{variable}' was not found.")
        {
            Data.Add(nameof(variable), variable);
        }
    }
}