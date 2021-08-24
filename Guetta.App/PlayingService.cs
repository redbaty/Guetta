using System;
using System.Threading;
using System.Threading.Tasks;
using Discord.Audio;
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
        
        private AudioOutStream CurrentDiscordStream { get; set; }
        
        private YoutubeDlService YoutubeDlService { get; }

        public double Volume { get; private set; } = 0.1f;
        
        internal IAudioClient AudioClient { get; private set; }
        
        private byte[] CurrentYoutubeAudioStream { get; set; }
        
        private byte[] CurrentFfmpegAudioStream { get; set; }
        
        private LocalisationService LocalisationService { get; }

        public async Task ChangeVolume(double newVolume, CancellationToken cancellationToken)
        {
            Volume = newVolume;
            
            if(CurrentFfmpegAudioStream != null)
                CurrentFfmpegAudioStream = await YoutubeDlService.GetAudioStream(CurrentYoutubeAudioStream, Volume, cancellationToken);
        }
        
        public void SetAudioClient(IAudioClient audioClient)
        {
            AudioClient?.Dispose();
            AudioClient = audioClient;
        }

        public async Task Play(QueueItem queueItem, CancellationToken cancellationToken)
        {
            if (CurrentDiscordStream != null)
            {
                await CurrentDiscordStream.FlushAsync(cancellationToken);
                await CurrentDiscordStream.DisposeAsync();
            }

            CurrentItem = queueItem;
            CurrentDiscordStream = AudioClient.CreatePCMStream(AudioApplication.Music);
            
            try
            {
                await LocalisationService.SendMessageAsync(CurrentItem.Channel, "SongDownloading",
                        CurrentItem.VideoInformation.Title, CurrentItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                await CurrentItem.Channel.TriggerTypingAsync();
                
                CurrentYoutubeAudioStream = await YoutubeDlService.GetYoutubeAudioStream(CurrentItem.YoutubeDlInput, cancellationToken);
                
                await LocalisationService.SendMessageAsync(CurrentItem.Channel, "SongPlaying",
                        CurrentItem.VideoInformation.Title, CurrentItem.User.Mention)
                    .DeleteMessageAfter(TimeSpan.FromSeconds(15));
                
                CurrentFfmpegAudioStream = await YoutubeDlService.GetAudioStream(CurrentYoutubeAudioStream, Volume, cancellationToken);
                
                for (var i = 0; i < CurrentFfmpegAudioStream.Length; i++)
                {
                    CurrentDiscordStream.WriteByte(CurrentFfmpegAudioStream[i]);
                    
                    if(cancellationToken.IsCancellationRequested)
                        break;
                }
            }
            finally
            {
                await CurrentDiscordStream.FlushAsync(CancellationToken.None);
                await CurrentDiscordStream.DisposeAsync();
                CurrentDiscordStream = null;
                CurrentYoutubeAudioStream = null;
                CurrentFfmpegAudioStream = null;
                CurrentItem = null;
            }
        }
    }
}