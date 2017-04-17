namespace MultiFileStream
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal sealed class SplittedStream : Stream
    {
        private readonly List<Stream> streams = new List<Stream>();
        private readonly Func<Stream> createNextStream;
        private readonly long streamLength;
        int currentStreamIndex = -1;

        public SplittedStream(long splitByLength, Func<Stream> createNextStream)
        {
            if (splitByLength <= 0) { throw new ArgumentOutOfRangeException(nameof(streamLength)); }

            this.streamLength = splitByLength;
            this.createNextStream = createNextStream;
        }

        public override bool CanWrite => true;

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!streams.Any())
            {
                SwitchToNextStream();
            }

            while (count > 0)
            {
                Stream currentSteam = streams[currentStreamIndex];
                int currentStreamLeft = (int)(streamLength - currentSteam.Position);
                bool needNextStream = count > currentStreamLeft;

                if (!needNextStream)
                {
                    currentSteam.Write(buffer, offset, count);
                    return;
                }

                currentSteam.Write(buffer, offset, currentStreamLeft);
                count = count - currentStreamLeft;
                offset = offset + currentStreamLeft;
                SwitchToNextStream();
            }
        }
        private void SwitchToNextStream()
        {
            streams.Add(createNextStream());
            currentStreamIndex++;
        }

        public override void Flush()
        {
            for (int i = 0; i <= currentStreamIndex; i++)
            {
                streams[i].Flush();
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (Stream stream in streams)
            {
                stream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override bool CanRead => false;
        public override int Read(byte[] buffer, int offset, int count) { throw new NotImplementedException(); }
        public override bool CanSeek => false;
        public override long Length { get { throw new NotImplementedException(); } }
        public override long Position { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }
        public override long Seek(long offset, SeekOrigin origin) { throw new NotImplementedException(); }
        public override void SetLength(long value) { throw new NotImplementedException(); }
    }
}
