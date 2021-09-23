﻿using System;
using System.Collections.Generic;

namespace Guetta.Abstractions
{
    public class CommandOptions
    {
        public Dictionary<string, Type> Commands { get; } = new();
        
        public string Prefix { get; set; } = "!";
    }
}