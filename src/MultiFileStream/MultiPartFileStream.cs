namespace MultiFileStream
{
    using System.Collections.Generic;
    using System.IO;

    public sealed class MultiPartFileStream : Stream
    {
        private readonly MultiPartFile multiPartFile;
        private readonly Stream stream;

        internal MultiPartFileStream(MultiPartFile multiPartFile, Stream stream)
        {
            this.multiPartFile = multiPartFile;
            this.stream = stream;
        }

        public IReadOnlyList<string> Files => multiPartFile.Files;

        protected override void Dispose(bool disposing)
        {
            stream.Dispose();
            base.Dispose(disposing);
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position { get { return stream.Position; } set { stream.Position = value; } }
        public override void Flush() => stream.Flush();
        public override int Read(byte[] buffer, int offset, int count) => stream.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);
        public override void SetLength(long value) => stream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);
    }
}
