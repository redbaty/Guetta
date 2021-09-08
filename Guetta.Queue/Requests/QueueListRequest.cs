﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Guetta.Abstractions.Converters;

namespace Guetta.Queue.Requests
{
    public class QueueListRequest
    {
        [JsonConverter(typeof(UlongConverter))]
        [Required]
        public ulong VoiceChannelId { get; set; }
    }
}