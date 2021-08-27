using System.Threading.Tasks;
using Guetta.Player.Requests;
using Guetta.Player.Services;
using Microsoft.AspNetCore.Mvc;

namespace Guetta.Player.Controllers
{
    [ApiController]
    [Route("/")]
    public class RootController : ControllerBase
    {
        public RootController(PlayingService playingService, PlayingServiceTokens playingServiceTokens)
        {
            PlayingService = playingService;
            PlayingServiceTokens = playingServiceTokens;
        }

        private PlayingService PlayingService { get; }

        private PlayingServiceTokens PlayingServiceTokens { get; }

        [HttpPost("play")]
        public async Task<ActionResult<string>> Play([FromBody] PlayRequest playRequest)
        {
            var isPlaying = PlayingService.Playing(playRequest.VoiceChannelId);

            if (isPlaying)
                return BadRequest("Already playing in this channel");

            return await PlayingService.Play(playRequest);
        }

        [HttpPost("playing")]
        public bool Playing([FromBody] PlayingRequest playingRequest) =>
            PlayingService.Playing(playingRequest.VoiceChannelId);

        [HttpPost("volume")]
        public Task SetVolume([FromBody] VolumeRequest volumeRequest) =>
            PlayingService.SetVolume(volumeRequest.VoiceChannelId, volumeRequest.Volume);

        [HttpPost("skip")]
        public bool Skip([FromBody] SkipRequest skipRequest) => PlayingServiceTokens.Cancel(skipRequest.VoiceChannelId);
    }
}