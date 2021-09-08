using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.VoiceNext;
using Guetta.App;
using Guetta.Player.Requests;
using StackExchange.Redis;

namespace Guetta.Player.Services
{
    public class PlayingService
    {
        public PlayingService(YoutubeDlService youtubeDlService,
            DiscordClient discordClient, PlayingServiceTokens playingServiceTokens, ISubscriber subscriber,
            IDatabase database)
        {
            YoutubeDlService = youtubeDlService;
            DiscordClient = discordClient;
            PlayingServiceTokens = playingServiceTokens;
            Subscriber = subscriber;
            Database = database;
        }

        private YoutubeDlService YoutubeDlService { get; }

        private PlayingServiceTokens PlayingServiceTokens { get; }

        private DiscordClient DiscordClient { get; }

        private static Dictionary<ulong, VoiceConnection> VoiceConnections { get; } = new();

        private static Dictionary<ulong, CurrentlyPlaying> CurrentlyPlaying { get; } = new();

        private IDatabase Database { get; }

        private ISubscriber Subscriber { get; }

        public bool Playing(ulong channel) =>
            VoiceConnections.ContainsKey(channel) &&
            CurrentlyPlaying.ContainsKey(channel);

        public async Task SetVolume(ulong channel, double volume)
        {
            await Database.HashSetAsync(channel.ToString(), "volume", volume);

            if (VoiceConnections.TryGetValue(channel, out var voiceConnection))
                voiceConnection.Sink.VolumeModifier = volume;
        }

        public async Task<string> Play(PlayRequest request)
        {
            if (Playing(request.VoiceChannelId))
                return null;

            var voiceChannel = await DiscordClient.GetChannelAsync(request.VoiceChannelId);

            if (!VoiceConnections.ContainsKey(request.VoiceChannelId))
            {
                var voiceNextConnection = await voiceChannel.ConnectAsync();
                VoiceConnections.Add(request.VoiceChannelId, new VoiceConnection
                {
                    Connection = voiceNextConnection,
                    Sink = voiceNextConnection.GetTransmitSink()
                });
            }

            var voiceConnection = VoiceConnections[request.VoiceChannelId];
            voiceConnection.Sink.VolumeModifier = request.InitialVolume;
            await SetVolume(request.VoiceChannelId, voiceConnection.Sink.VolumeModifier);

            if (!CurrentlyPlaying.ContainsKey(request.VoiceChannelId))
            {
                CurrentlyPlaying.Add(request.VoiceChannelId, new CurrentlyPlaying
                {
                    Request = request,
                    Id = await Nanoid.Nanoid.GenerateAsync()
                });
            }

            var currentlyPlaying = CurrentlyPlaying[request.VoiceChannelId];
            var cancellationToken = PlayingServiceTokens.GetCancellationToken(request.VoiceChannelId);
            
            _ = YoutubeDlService.SendToAudioSink(request.VideoInformation.Url, voiceConnection.Sink, cancellationToken)
                .ContinueWith(
                    async t =>
                    {
                        await voiceConnection.Sink.FlushAsync(CancellationToken.None);

                        PlayingServiceTokens.Remove(request.VoiceChannelId);
                        CurrentlyPlaying.Remove(request.VoiceChannelId);
                        await Subscriber.PublishAsync($"{currentlyPlaying.Id}:ended", t.IsCompletedSuccessfully);
                    }, CancellationToken.None);
            
            return currentlyPlaying.Id;
        }
    }
}