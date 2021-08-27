﻿using System.Text.Json.Serialization;
using Guetta.Player.Converters;

namespace Guetta.Player.Requests
{
    public class SkipRequest
    {
        [JsonConverter(typeof(UlongConverter))]
        public ulong VoiceChannelId { get; set; }
    }
}