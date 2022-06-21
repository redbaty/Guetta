using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Localisation.Resources;
using Microsoft.Extensions.Options;

namespace Guetta.Localisation
{
    public class LocalisationService
    {
        public LocalisationService(IOptions<LocalisationOptions> options)
        {
            Language.Culture = new CultureInfo(options.Value.Language);
            
            Items = typeof(Language)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(i => i.CanRead && !i.CanWrite && i.PropertyType == typeof(string))
                .Select(i => new { i.Name, Valor = i.GetValue(null)?.ToString() })
                .Where(i => i.Valor != null)
                .ToDictionary(i => i.Name, i => i.Valor!);
        }

        private Dictionary<string, string> Items { get; }

        public string GetMessageTemplate(string code)
        {
            var messageTemplate = Items.GetValueOrDefault(code);

            if (string.IsNullOrEmpty(messageTemplate))
            {
                throw new ArgumentOutOfRangeException(nameof(code));
            }

            return messageTemplate;
        }
        
        public Task<DiscordMessage> ReplyMessageAsync(DiscordMessage message, string code,
            params object[] parameters)
        {
            return message.RespondAsync(string.Format(GetMessageTemplate(code), parameters));
        }
        
        public Task<DiscordMessage> SendMessageAsync(DiscordChannel channel, string code,
            params object[] parameters)
        {
            return channel.SendMessageAsync(string.Format(GetMessageTemplate(code), parameters));
        }
    }
}