using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Guetta.Abstractions;
using MessagePack;

namespace Guetta.Queue.Abstractions
{
    [MessagePackObject]
    public class PlayRequest : IValidatableObject, IPlayRequest
    {
        [MessagePack.Key(0)]
        [JsonPropertyName("voiceChannelId")]
        public string VoiceChannelId { get; set; }

        [MessagePack.Key(1)]
        [JsonPropertyName("videoInformation")]
        public VideoInformation VideoInformation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if(VoiceChannelId == default)
                yield return new ValidationResult("VoiceChannelId can't be 0", new[] { nameof(VoiceChannelId) });

            if(VideoInformation == null)
                yield return new ValidationResult("No Video Information provided", new[] { nameof(VideoInformation) });
        }
    }
}