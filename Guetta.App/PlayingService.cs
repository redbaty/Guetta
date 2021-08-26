using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext;
using Guetta.Abstractions;
using Guetta.App.Extensions;
using Guetta.Localisation;

namespace Guetta.App
{
    public class PlayingService
    {
        public PlayingService(YoutubeDlService youtubeDlService, LocalisationService localisationService)
        {
            YoutubeDlService = youtubeDlService;
            LocalisationService = localisationService;
        }

        private QueueItem CurrentItem { get; set; }
        
        private VoiceTransmitSink CurrentDiscordSink { get; set; }
        
        private YoutubeDlService YoutubeDlService { get; }

        internal VoiceNextConnection AudioClient { get; private set; }
        
        private byte[] CurrentYoutubeAudioStream { get; set; }
        
        private byte[] CurrentFfmpegAudioStream { get; set; }
        
        private LocalisationService LocalisationService { get; }

        public Task ChangeVolume(double newVolume)
        {
            CurrentDiscordSink.VolumeModifier = newVolume;
            return Task.CompletedTask;
        }
        
        public void SetAudioClient(VoiceNextConnection audioClient)
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
                await LocalisationService.SendMessageAsync(CurrentItem.Channel, "SongDownloading",
                        CurrentItem.VideoInformation.Title, CurrentItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                await CurrentItem.Channel.TriggerTypingAsync();
                
                await LocalisationService.SendMessageAsync(CurrentItem.Channel, "SongPlaying",
                        CurrentItem.VideoInformation.Title, CurrentItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                
                await YoutubeDlService.SendToAudioSink(CurrentItem.YoutubeDlInput, CurrentDiscordSink, cancellationToken);
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