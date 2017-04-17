﻿namespace MultiFileStream
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal sealed class ConcatenatedStream : Stream
    {
        readonly Queue<Stream> streams;

        public ConcatenatedStream(IEnumerable<Stream> streams)
        {
            this.streams = new Queue<Stream>(streams);
        }

        public override bool CanRead => true;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (streams.Count == 0)
            {
                return 0;
            }

            int bytesRead = streams.Peek().Read(buffer, offset, count);
            if (bytesRead == 0)
            {
                streams.Dequeue().Dispose();
                bytesRead += Read(buffer, offset + bytesRead, count - bytesRead);
            }
            return bytesRead;
        }

        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override void Flush() { throw new NotImplementedException(); }
        public override long Length { get { throw new NotImplementedException(); } }
        public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void SetLength(long value) { throw new NotImplementedException(); }
        public override void Write(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
    }
}
