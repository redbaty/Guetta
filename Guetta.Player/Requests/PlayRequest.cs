using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Guetta.Abstractions;
using Guetta.Abstractions.Converters;

namespace Guetta.Player.Requests
{
    public class PlayRequest : IValidatableObject, IPlayRequest
    {
        [JsonConverter(typeof(UlongConverter))]
        public ulong VoiceChannelId { get; set; }
        
        public double InitialVolume { get; set; }

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