using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;

namespace ChatServer
{
    public class StreamReader
    {
        private byte[] remaining = new byte[0];
        private readonly Stream stream;

        public StreamReader(Stream stream)
        {
            this.stream = stream;
        }

        public void ReadShort(Action<short> callback)
        {
            GetData(2, r =>
            {
                if (r.Length == 0)
                    callback(0);
                else
                    callback(BitConverter.ToInt16(r));
            });
        }

        public void ReadString(short size, Action<string> callback)
        {
            GetData(size, r => callback(Encoding.ASCII.GetString(r)));
        }

        public void WriteShort(short value)
        {
            WriteData(BitConverter.GetBytes(value));
        }

        public void WriteData(byte[] value)
        {
            stream.BeginWrite(value, 0, value.Length, r =>
            {
                stream.EndWrite(r);
            }, null);
        }

        public void Close()
        {
            stream.Close();
        }

        public void GetData(int length, Action<byte[]> callback)
        {
            var buffer = new byte[length];
            if (CheckRemaining())
            {
                Array.Resize(ref buffer, buffer.Length + remaining.Length);
                Array.Copy(remaining, buffer, remaining.Length);
            }
            
            var abc = stream.BeginRead(buffer, remaining.Length, buffer.Length - remaining.Length, r =>
            {
                int bytesReceived = stream.EndRead(r);
                if (bytesReceived == 0)
                    callback(new byte[0]);
                Array.Resize(ref buffer, bytesReceived + remaining.Length);
                Array.Resize(ref remaining, 0);

                if (buffer.Length < length)
                    GetData(length - buffer.Length, s => callback(Concat(buffer, s)));
                else
                {
                    buffer = TreatDataOverflow(length, buffer);
                    callback(buffer);
                }
            }, null);
        }

        private byte[] Concat (byte[] first, byte[] second)
        {
            var result = new byte[first.Length + second.Length];
            Array.Copy(first, 0, result, 0, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }

        private byte[] TreatDataOverflow(int length, byte[] data)
        {
            Array.Resize(ref remaining, data.Length - length);
            Array.Copy(data, length, remaining, 0, remaining.Length);
            Array.Resize(ref data, length);
            return data;
        }

        private bool CheckRemaining()
            => (Object.Equals(remaining, null) || remaining.Length == 0) ? false : true;
    }
}