using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
using Guetta.App;
using Guetta.App.Extensions;
using Guetta.Localisation;
using Guetta.Player.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Guetta.Player
{
    public class PlayingServiceTokens
    {
        private Dictionary<ulong, CancellationTokenSource> CancellationTokenSources { get; } = new();

        public CancellationToken GetCancellationToken(ulong channel)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            CancellationTokenSources.Add(channel, cancellationTokenSource);
            return cancellationTokenSource.Token;
        }

        internal void Remove(ulong channel)
        {
            if (!CancellationTokenSources.ContainsKey(channel))
                return;

            CancellationTokenSources.Remove(channel);
        }

        public bool Cancel(ulong channel)
        {
            if (!CancellationTokenSources.ContainsKey(channel))
                return false;

            var cancellationTokenSource = CancellationTokenSources[channel];
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            CancellationTokenSources.Remove(channel);

            return true;
        }
    }

    public class PlayingService
    {
        public PlayingService(YoutubeDlService youtubeDlService, LocalisationService localisationService,
            DiscordClient discordClient, PlayingServiceTokens playingServiceTokens)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
            DiscordClient = discordClient;
            PlayingServiceTokens = playingServiceTokens;
        }

        private YoutubeDlService YoutubeDlService { get; }

        private LocalisationService LocalisationService { get; }

        private PlayingServiceTokens PlayingServiceTokens { get; }

        private DiscordClient DiscordClient { get; }

        private Dictionary<ulong, VoiceNextConnection> VoiceConnections { get; } = new();

        public bool Playing(ulong channel)
        {
            return VoiceConnections.TryGetValue(channel, out var voiceChannelConnection) &&
                   voiceChannelConnection.IsPlaying;
        }

        public async Task<bool> Play(IPlayRequest request)
        {
            var voiceChannel = await DiscordClient.GetChannelAsync(request.VoiceChannelId);
            var textChannel = await DiscordClient.GetChannelAsync(request.TextChannelId);


            if (!VoiceConnections.ContainsKey(request.VoiceChannelId))
            {
                VoiceConnections.Add(request.VoiceChannelId, await voiceChannel.ConnectAsync());
            }

            var voiceNextConnection = VoiceConnections[request.VoiceChannelId];
            var voiceTransmitSink = voiceNextConnection.GetTransmitSink();

            if (voiceNextConnection.IsPlaying)
            {
                return false;
            }

            var cancellationToken = PlayingServiceTokens.GetCancellationToken(request.VoiceChannelId);

            await LocalisationService.SendMessageAsync(textChannel, "SongDownloading",
                    request.VideoInformation.Title, request.RequestedByUser)
                .DeleteMessageAfter(TimeSpan.FromSeconds(15));
            await textChannel.TriggerTypingAsync();

            await LocalisationService.SendMessageAsync(textChannel, "SongPlaying",
                    request.VideoInformation.Title, request.RequestedByUser)
                .DeleteMessageAfter(TimeSpan.FromSeconds(15));
            
            var playbackStart = new TaskCompletionSource<bool>();

            _ = YoutubeDlService.SendToAudioSink(request.Input, voiceTransmitSink, cancellationToken, playbackStart).ContinueWith(
                async _ =>
                {
                    PlayingServiceTokens.Remove(request.VoiceChannelId);
                    await voiceTransmitSink.FlushAsync(CancellationToken.None);
                }, CancellationToken.None);

            await playbackStart.Task;
            return true;
        }
    }
}