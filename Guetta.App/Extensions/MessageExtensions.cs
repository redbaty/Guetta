using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace Guetta.App.Extensions
{
    public static class MessageExtensions
    {
        public static async Task<bool> AskReply(this DiscordMessage message, string content, DiscordUser user, string positiveContent, DiscordEmoji positiveEmoji, string negativeContent, DiscordEmoji negativeEmoji, TimeSpan? timeoutOverride = null)
        {
            var positiveButton = new DiscordButtonComponent(ButtonStyle.Success, "btn_success", positiveContent, false, new DiscordComponentEmoji(positiveEmoji));
            var negativeButton = new DiscordButtonComponent(ButtonStyle.Secondary, "btn_negative", negativeContent, false, new DiscordComponentEmoji(negativeEmoji));
            var discordMessage = await message.RespondAsync(b => b.WithContent(content).AddComponents(negativeButton, positiveButton));

            var waitForButtonAsync = await discordMessage.WaitForButtonAsync(user, timeoutOverride);
            await discordMessage.DeleteAsync();
            return !waitForButtonAsync.TimedOut && waitForButtonAsync.Result.Id == "btn_success";
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