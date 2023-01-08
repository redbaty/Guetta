using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using DSharpPlus.VoiceNext;
using Emzi0767.Utilities;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Microsoft.Extensions.Logging;

namespace Guetta.App
{
    internal record PlayRequest(QueueItem QueueItem, CancellationTokenSource CancellationToken);

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
        

        private YoutubeDlService YoutubeDlService { get; }

        public VoiceNextConnection AudioClient { get; set; }

        public ulong? ChannelId => AudioClient?.TargetChannel?.Id;

        public bool IsPlaying => AudioClient is { IsPlaying: true } && !AudioClient.IsDisposed();

        private LocalisationService LocalisationService { get; }

        private ILogger<Voice> Logger { get; }

        public Task ChangeVolume(double newVolume)
        {
            if(AudioClient?.GetTransmitSink() is { } transmitSink)
                transmitSink.VolumeModifier = newVolume;
            
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
            if (AudioClient != null || AudioClient.IsDisposed())
            {
                if (AudioClient != null && !AudioClient.IsDisposed())
                {
                    if (AudioClient.IsPlaying)
                    {
                        Logger.LogInformation("Going to wait for playback to finish");
                        
                        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
                        var waitForFinishTask = AudioClient.WaitForPlaybackFinishAsync();
                        var endedTask = await Task.WhenAny(waitForFinishTask, timeoutTask);

                        if (endedTask == timeoutTask)
                        {
                            Logger.LogWarning("Waiting for playback to finish timed out");
                        }
                    }

                    AudioClient.Disconnect();
                }

                AudioClient = null;
            }
        }

        public Task Play(QueueItem queueItem, CancellationTokenSource cancellationToken) => Play(new PlayRequest(queueItem, cancellationToken));

        private async Task Play(PlayRequest playRequest)
        {
            var CurrentDiscordSink = AudioClient.GetTransmitSink();
            var webSocketClient = AudioClient.GetWebsocket();

            Task WebSocketClientOnDisconnected(IWebSocketClient sender, SocketCloseEventArgs args)
            {
                if (!args.Handled && args.CloseCode == 4014)
                {
                    playRequest.CancellationToken.Cancel();
                }

                return Task.CompletedTask;
            }

            webSocketClient.Disconnected += WebSocketClientOnDisconnected;

            try
            {
                await playRequest.QueueItem.TextChannel.TriggerTypingAsync();

                await LocalisationService.SendMessageAsync(playRequest.QueueItem.TextChannel, "SongPlaying",
                        playRequest.QueueItem.VideoInformation.Title, playRequest.QueueItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));

                await YoutubeDlService.SendToAudioSink(playRequest.QueueItem.VideoInformation.Url, CurrentDiscordSink, playRequest.CancellationToken.Token);
            }
            catch (Exception ex)
            {
                if (!playRequest.CancellationToken.IsCancellationRequested)
                    Logger.LogError(ex, "Failed to play something");
            }
            finally
            {
                webSocketClient.Disconnected -= WebSocketClientOnDisconnected;

                if (CurrentDiscordSink != null)
                {
                    if (!AudioClient.IsDisposed())
                        await CurrentDiscordSink.FlushAsync(playRequest.CancellationToken.Token).ContinueWith(_ => { });
                }
            }
        }
    }
}