using DSharpPlus;
using FluentValidation;
using Guetta.Player.Requests;

namespace Guetta.Player.Validators
{
    public class PlayRequestValidator : AbstractValidator<PlayRequest>
    {
        public PlayRequestValidator(DiscordClient discordClient)
        {
            RuleFor(i => i.VoiceChannelId)
                .MustAsync(async (voiceChannelId, _) =>
                {
                    var channel = await discordClient.GetChannelAsync(voiceChannelId).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);
                    return channel != null;
                })
                .WithMessage("Voice channel does not exist.");
            
            RuleFor(i => i.TextChannelId)
                .MustAsync(async (textChannelId, _) =>
                {
                    var channel = await discordClient.GetChannelAsync(textChannelId).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);
                    return channel != null;
                })
                .WithMessage("Text channel does not exist.");
        }
    }
}