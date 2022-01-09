using System;
using System.IO;
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
    public class Voice
    {
        public Voice(YoutubeDlService youtubeDlService, LocalisationService localisationService, ulong guildId, ILogger<Voice> logger)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
            GuildId = guildId;
            Logger = logger;
        }

        private ulong GuildId { get; }

        internal QueueItem CurrentItem { get; set; }

        private VoiceTransmitSink CurrentDiscordSink { get; set; }

        private YoutubeDlService YoutubeDlService { get; }

        internal VoiceNextConnection AudioClient { get; private set; }

        private byte[] CurrentYoutubeAudioStream { get; set; }

        private byte[] CurrentFfmpegAudioStream { get; set; }

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
            CurrentItem = queueItem;
            CurrentDiscordSink ??= AudioClient.GetTransmitSink();

            try
            {
                await CurrentItem.Channel.TriggerTypingAsync();

                await LocalisationService.SendMessageAsync(CurrentItem.Channel, "SongPlaying",
                        CurrentItem.VideoInformation.Title, CurrentItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));

                await YoutubeDlService.SendToAudioSink(CurrentItem.VideoInformation.Url, CurrentDiscordSink, cancellationToken);
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.LogError(ex, "Failed to play something");
            }
            finally
            {
                await CurrentDiscordSink.FlushAsync(CancellationToken.None);
                CurrentYoutubeAudioStream = null;
                CurrentFfmpegAudioStream = null;
                CurrentItem = null;
            }
        }
    }
}