using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Guetta.Abstractions;
using Guetta.Player.Abstractions;

namespace Guetta.Player.Controllers
{
    public class PlayRequest : IPlayRequest, IValidatableObject
    {
        [JsonConverter(typeof(UlongConverter))]
        public ulong TextChannelId { get; set; }
        
        [JsonConverter(typeof(UlongConverter))]
        public ulong VoiceChannelId { get; set; }

        public string RequestedByUser { get; set; }
        
        public string Input { get; set; }
        
        public VideoInformation VideoInformation { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(Input))
                yield return new ValidationResult("Input can't be null or empty", new[] { nameof(Input) });
        }
    }
}