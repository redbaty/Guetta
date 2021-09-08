﻿using System.Text.Json.Serialization;
using Guetta.Abstractions.Converters;

namespace Guetta.Player.Requests
{
    public class VolumeRequest
    {
        [JsonConverter(typeof(UlongConverter))]
        public ulong VoiceChannelId { get; set; }
        
        public double Volume { get; set; }
    }
}