using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Guetta.Abstractions;
using Guetta.Localisation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guetta.App
{
    public class CommandSolverService
    {
        private ILogger<CommandSolverService> Logger { get; }

        private Channel<DiscordMessage> ExecutionChannel { get; } = Channel.CreateBounded<DiscordMessage>(100);

        public CommandSolverService(IServiceProvider provider, ILogger<CommandSolverService> logger, IOptions<CommandOptions> options, LocalisationService localisationService)
        {
            Provider = provider;
            Logger = logger;
            LocalisationService = localisationService;
            CommandOptions = options.Value;
        }

        private CommandOptions CommandOptions { get; }

        private IServiceProvider Provider { get; }
        
        private LocalisationService LocalisationService { get; }

        public Task CreateCommandQueueTask()
        {
            return Task.Run(async () =>
            {
                await foreach (var message in ExecutionChannel.Reader.ReadAllAsync())
                {
                    var commandArguments = message.Content[1..].Split(' ');
                    var discordCommand = GetCommand(commandArguments.First().ToLower());

                    if (discordCommand != null)
                        await discordCommand.ExecuteAsync(message, commandArguments.Skip(1).ToArray())
                            .ContinueWith(t =>
                            {
                                if (t.IsFaulted)
                                {
                                    Logger.LogError(t.Exception, "Error while running command {@Command}", discordCommand);
                                }
                            });
                    else
                        await LocalisationService.ReplyMessageAsync(message, "InvalidCommand");
                }
            });
        }

        public ValueTask AddMessageToQueue(DiscordMessage message)
        {
            return ExecutionChannel.Writer.WriteAsync(message);
        }

        private IDiscordCommand GetCommand(string command)
        {
            Logger.LogInformation("Command received: {@Command}", command);

            if (CommandOptions.Commands.TryGetValue(command, out var commandType))
            {
                Logger.LogDebug("Command {@Command} solved to type {@CommandType}", command, commandType.Name);

                return (IDiscordCommand)Provider.GetService(commandType);
            }

            return null;
        }
    }
}