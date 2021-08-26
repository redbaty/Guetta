using Guetta.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Guetta.Commands.Extensions
{
    public static class ServiceExtensions
    {
        public static void WithPrefix(this IServiceCollection serviceCollection, string prefix)
        {
            serviceCollection.Configure<CommandOptions>(o =>
            {
                o.Prefix = prefix;
            });
        }
        
        public static void AddCommand<T>(this IServiceCollection serviceCollection, string command) where T : class, IDiscordCommand
        {
            serviceCollection.AddTransient<T>();
            serviceCollection.Configure<CommandOptions>(o =>
            {
                o.Commands.Add(command.ToLower(), typeof(T));
            });
        }
    }
}