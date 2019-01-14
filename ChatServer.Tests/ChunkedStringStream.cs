using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ChatServer.Tests
{
    class ChunkedStringStream : Stream
    {
        private byte[] content;
        private int position;

        public ChunkedStringStream(params string[] content)
        {

            this.content = new byte[0];
            foreach (var c in content)
            {
                var size = BitConverter.GetBytes((short)c.Length);
                var actualContent = Encoding.ASCII.GetBytes(c);
                int index = this.content.Length;
                Array.Resize(ref this.content, this.content.Length + size.Length + actualContent.Length);
                size.CopyTo(this.content, index);
                actualContent.CopyTo(this.content, index + size.Length);
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => content.Length;

        public override long Position { get => position; set => position = (int)value; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            buffer[offset] = content[position++];
            return 1;

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
    }
}
