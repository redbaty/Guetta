using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Microsoft.Extensions.Logging;

namespace Guetta.App
{
    public class Voice : IGuildItem
    {
        public Voice(YoutubeDlService youtubeDlService, LocalisationService localisationService, ulong guildId, ILogger<Voice> logger)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
            GuildId = guildId;
            Logger = logger;
        }

        public ulong GuildId { get; }

        private VoiceTransmitSink CurrentDiscordSink { get; set; }

        private YoutubeDlService YoutubeDlService { get; }

        internal VoiceNextConnection AudioClient { get; private set; }

        private LocalisationService LocalisationService { get; }

        private ILogger<Voice> Logger { get; }

        public Task ChangeVolume(double newVolume)
        {
            CurrentDiscordSink.VolumeModifier = newVolume;
            return Task.CompletedTask;
        }

        public async Task Join(DiscordChannel voiceChannel)
        {
            var audioClient = await voiceChannel.ConnectAsync();
            SetAudioClient(audioClient);
        }

        private void SetAudioClient(VoiceNextConnection audioClient)
        {
            AudioClient?.Dispose();
            AudioClient = audioClient;
        }

        public async Task Play(QueueItem queueItem, CancellationToken cancellationToken)
        {
            CurrentDiscordSink ??= AudioClient.GetTransmitSink();

            try
            {
                await queueItem.Channel.TriggerTypingAsync();

                await LocalisationService.SendMessageAsync(queueItem.Channel, "SongPlaying",
                        queueItem.VideoInformation.Title, queueItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));

                await YoutubeDlService.SendToAudioSink(queueItem.VideoInformation.Url, CurrentDiscordSink, cancellationToken);
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.LogError(ex, "Failed to play something");
            }
            finally
            {
                await CurrentDiscordSink.FlushAsync(CancellationToken.None);
                queueItem = null;
            }
        }
    }
}