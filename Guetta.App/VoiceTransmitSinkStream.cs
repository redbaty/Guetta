using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.VoiceNext;

namespace Guetta.App
{
    public class VoiceTransmitSinkStream : Stream
    {
        public VoiceTransmitSinkStream(VoiceTransmitSink sink, TaskCompletionSource<bool> playbackStarted)
        {
            Sink = sink;
            PlaybackStarted = playbackStarted;
        }

        private VoiceTransmitSink Sink { get; }
        
        private TaskCompletionSource<bool> PlaybackStarted { get; set; }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Sink.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (PlaybackStarted != null)
            {
                PlaybackStarted.SetResult(true);
                PlaybackStarted = null;
            }
            
            await Sink.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
        }
    }
}