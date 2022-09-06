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

        private VoiceNextConnection AudioClient { get; set; }

        public ulong? ChannelId => AudioClient?.TargetChannel?.Id;

        public bool IsPlaying => AudioClient?.IsPlaying ?? false;

        private LocalisationService LocalisationService { get; }

        private ILogger<Voice> Logger { get; }

        public Task ChangeVolume(double newVolume)
        {
            CurrentDiscordSink.VolumeModifier = newVolume;
            return Task.CompletedTask;
        }

        public async Task Join(DiscordChannel voiceChannel)
        {
            if (AudioClient != null && AudioClient.TargetChannel.Id == voiceChannel.Id)
                return;
            
            if (AudioClient != null && AudioClient.TargetChannel.Id != voiceChannel.Id)
            {
                await Disconnect();
            }

            var audioClient = await voiceChannel.ConnectAsync();
            AudioClient = audioClient;
        }

        public async Task Disconnect()
        {
            if (AudioClient != null)
            {
                await AudioClient.WaitForPlaybackFinishAsync();
                AudioClient.Disconnect();
                AudioClient = null;
            }

            if (CurrentDiscordSink != null)
            {
                CurrentDiscordSink.Dispose();
                CurrentDiscordSink = null;
            }
        }

        public async Task Play(QueueItem queueItem, CancellationToken cancellationToken)
        {
            CurrentDiscordSink ??= AudioClient.GetTransmitSink();

            try
            {
                await queueItem.TextChannel.TriggerTypingAsync();

                await LocalisationService.SendMessageAsync(queueItem.TextChannel, "SongPlaying",
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
            }
        }
    }
}