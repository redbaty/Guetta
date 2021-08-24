using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Guetta.Localisation
{
    public class LocalisationService
    {
        private readonly IOptions<LocalisationOptions> _options;

        public LocalisationService(IOptions<LocalisationOptions> options)
        {
            _options = options;
        }

        private IOptions<LocalisationOptions> Options => _options;

        internal static Dictionary<string, Dictionary<string, string>> Items { get; } = new();

        private const string DefaultLanguage = "en";
        

        public async Task<RestUserMessage> SendMessageAsync(ISocketMessageChannel channel, string code,
            params object[] parameters)
        {
            var messageTemplate = //Items.GetValueOrDefault(Options.Value.Language)?.GetValueOrDefault(code) ??
                Items[DefaultLanguage].GetValueOrDefault(code);

            if (string.IsNullOrEmpty(messageTemplate))
            {
                throw new ArgumentOutOfRangeException(nameof(code));
            }

            var message = string.Format(messageTemplate, parameters);
            return await channel.SendMessageAsync(message);
        }
    }
}