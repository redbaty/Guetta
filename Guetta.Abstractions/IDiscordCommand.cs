using System.Threading.Tasks;
using Discord.WebSocket;

namespace Guetta.Abstractions
{
    public interface IDiscordCommand
    {
        Task ExecuteAsync(SocketMessage message, string[] arguments);
    }
}