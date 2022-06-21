using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Guetta.App.Extensions
{
    public static class MessageExtensions
    {
        public static async Task<bool> Ask(this DiscordMessage message, DiscordUser user, DiscordEmoji positiveEmoji, DiscordEmoji negativeEmoji, TimeSpan? timeoutOverride = null)
        {
            await message.AddReactions(new[] { negativeEmoji, positiveEmoji });
            var reaction = await message.WaitForReactionAsync(user, timeoutOverride);
            return !reaction.TimedOut && reaction.Result.Emoji == positiveEmoji;
        }

        private static async Task AddReactions(this DiscordMessage message, IEnumerable<DiscordEmoji> emojis)
        {
            foreach (var emoji in emojis) await message.CreateReactionAsync(emoji);
        }
        
        public static Task DeleteMessageAfter(this DiscordMessage message, TimeSpan timeout)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(timeout);
                await message.DeleteAsync().ContinueWith(_ => Task.CompletedTask);
            });
        }


        public static Task DeleteMessageAfter(this Task<DiscordMessage> message, TimeSpan timeout)
        {
            return message.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(timeout);
                        await t.Result.DeleteAsync();
                    });
                }
                else
                {
                    throw t.Exception ?? new Exception("Failed to send message");
                }
            });
        }
    }
}