using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Guetta.Abstractions;
using Microsoft.Extensions.Logging;

namespace Guetta.App
{
    public class YoutubeDlService
    {
        public YoutubeDlService(ILogger<YoutubeDlService> logger)
        {
            Logger = logger;
        }

        private ILogger<YoutubeDlService> Logger { get; }

        public async Task<VideoInformation> GetVideoInformation(string input, CancellationToken cancellationToken)
        {
            var youtubeDlArguments = new[]
            {
                "-J",
                $"\"{input}\""
            };
            
            var youtubeDlCommand = Cli.Wrap("youtube-dl")
                .WithArguments(youtubeDlArguments, false);

            Logger.LogInformation("Calling youtube-dl with args: {@Arguments}", youtubeDlCommand);

            var exec = await youtubeDlCommand.ExecuteBufferedAsync(Encoding.UTF8, cancellationToken);
            var jsonDocument = JsonDocument.Parse(exec.StandardOutput);
            
            Logger.LogInformation("Youtube-DL ran, and results were parsed");
            
            var rootElement = jsonDocument.RootElement;

            if (rootElement.TryGetProperty("entries", out var entriesElement) &&
                entriesElement is { ValueKind: JsonValueKind.Array })
            {
                if (entriesElement.EnumerateArray().SingleOrDefault() is var firstEntryElement)
                {
                    return CreateYoutubeVideoInformation(firstEntryElement);
                }
            }

            return CreateYoutubeVideoInformation(rootElement);
        }

        private static VideoInformation CreateYoutubeVideoInformation(JsonElement rootElement) =>
            new()
            {
                Url = $"https://www.youtube.com/watch?v={rootElement.GetProperty("id").GetString()}",
                Title = rootElement.GetProperty("title").GetString()
            };
    }
}