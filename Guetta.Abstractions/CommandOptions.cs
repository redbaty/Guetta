using System;
using System.Collections.Generic;
using System.Linq;

namespace Guetta.Abstractions
{
    public class CommandOptions
    {
        public Dictionary<string, Type> Commands { get; } = new();

        public char Prefix { get; set; } = Environment.GetEnvironmentVariable("COMMAND_PREFIX")?.First() ?? '!';
    }
}