using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Microsoft.Extensions.Options;

namespace Guetta.Commands
{
    public class HelpCommand : IDiscordCommand
    {
        public HelpCommand(IOptions<CommandOptions> options)
        {
            Options = options;
        }

        private IOptions<CommandOptions> Options { get; }

        public async Task ExecuteAsync(DiscordMessage message, string[] arguments)
        {
            var commandOptions = Options.Value;

            foreach (var command in Options.Value.Commands.Keys)
                await message.Channel.SendMessageAsync($"{commandOptions.Prefix}{command}");
        }
    }
}