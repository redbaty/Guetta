using System.Threading.Tasks;
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
        public Task<bool> Play([FromBody] PlayRequest playRequest) => PlayingService.Play(playRequest);

        [HttpPost("playing")]
        public bool Playing([FromBody] SkipRequest skipRequest) => PlayingService.Playing(skipRequest.VoiceChannelId);

        [HttpPost("skip")]
        public bool Skip([FromBody] SkipRequest skipRequest) => PlayingServiceTokens.Cancel(skipRequest.VoiceChannelId);
    }
}