using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Guetta.App
{
    public class ChannelStream : Stream
    {
        private Channel<byte[]> UnderlyingChannel { get; } = Channel.CreateBounded<byte[]>(50);

        private ChannelWriter<byte[]> Writer => UnderlyingChannel.Writer;

        public ChannelReader<byte[]> Reader => UnderlyingChannel.Reader;

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get; set; }

        public void Complete()
        {
            Writer.Complete();
        }

        public override void Flush()
        {
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

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new())
        {
            return Writer.WriteAsync(buffer.ToArray(), cancellationToken);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Writer.WriteAsync(buffer[offset..count], cancellationToken).AsTask();
        }
    }
}