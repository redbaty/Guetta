using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Guetta.Abstractions
{
    public interface IDiscordCommand
    {
        Task ExecuteAsync(DiscordMessage message, string[] arguments);
    }
}