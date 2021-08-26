using System;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Guetta.App
{
    public class CommandSolverService
    {
        private ILogger<CommandSolverService> Logger { get; }

        public CommandSolverService(IServiceProvider provider, ILogger<CommandSolverService> logger, IOptions<CommandOptions> options)
        {
            Provider = provider;
            Logger = logger;
            CommandOptions = options.Value;
        }
        
        private CommandOptions CommandOptions { get; }

        private IServiceProvider Provider { get; }

        public IDiscordCommand GetCommand(string command)
        {
            Logger.LogInformation("Command received: {@Command}", command);

            if (CommandOptions.Commands.TryGetValue(command, out var commandType))
            {
                Logger.LogDebug("Command {@Command} solved to type {@CommandType}", command, commandType.Name);

                return (IDiscordCommand) Provider.GetService(commandType);
            }

            throw new NotImplementedException();
        }
    }
}