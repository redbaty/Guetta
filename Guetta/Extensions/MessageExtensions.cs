﻿using System;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace Guetta.Extensions
{
    internal static class MessageExtensions
    {
        public static Task DeleteMessageAfter(this SocketMessage message, TimeSpan timeout)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(timeout);
                await message.DeleteAsync();
            });
        }


        public static Task DeleteMessageAfter(this Task<RestUserMessage> message, TimeSpan timeout)
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