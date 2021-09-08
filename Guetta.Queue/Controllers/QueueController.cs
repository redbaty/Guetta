using System.Collections.Generic;
using System.Threading.Tasks;
using Guetta.Queue.Abstractions;
using Guetta.Queue.Models;
using Guetta.Queue.Requests;
using Guetta.Queue.Services;
using Microsoft.AspNetCore.Mvc;

namespace Guetta.Queue.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        public QueueController(QueueService queueService, QueueStatusService queueStatusService)
        {
            QueueService = queueService;
            QueueStatusService = queueStatusService;
        }

        private QueueService QueueService { get; }

        private QueueStatusService QueueStatusService { get; }

        [HttpGet("status")]
        public Task<QueueStatus> GetQueueStatus([FromQuery] QueueStatusRequest request) =>
            QueueStatusService.GetQueueStatus(request.VoiceChannelId);

        [HttpGet("items")]
        public IAsyncEnumerable<PlayRequest> GetQueueItems([FromQuery] QueueListRequest request) =>
            QueueService.GetQueueItems(request.VoiceChannelId);

        [HttpPost("skip")]
        public Task Skip([FromBody] QueueSkipRequest queueSkipRequest) =>
            QueueService.Skip(queueSkipRequest.VoiceChannelId);

        [HttpPost]
        public async Task<long?> Post([FromBody] QueueItem queueItem) =>
            await QueueService.Enqueue(queueItem.VoiceChannelId, queueItem);
    }
}