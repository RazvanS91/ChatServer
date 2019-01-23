using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;

namespace ChatServer
{
    public class StreamReader
    {
        private byte[] data;
        private byte[] dataFromClient;
        private byte[] remaining;
        private readonly Stream stream;

        public StreamReader(Stream stream)
        {
            this.stream = stream;
            data = new byte[12];
        }

        public short ReadShort()
        {
            var data = GetData(2);
            return BitConverter.ToInt16(data);
        }

        public string ReadString(short size)
        {
            var data = GetData(size);
            return Encoding.ASCII.GetString(data);
        }

        public void WriteShort(short value)
        {
            WriteData(BitConverter.GetBytes(value));
        }

        public void WriteData(byte[] value)
        {
            stream.Write(value);
        }

        public void Close()
        {
            stream.Close();
        }

        public byte[] GetData(int length)
        {
            if (CheckRemaining())
            {
                dataFromClient = remaining;
                if(dataFromClient.Length >= length)
                    return TreatDataOverflow(length);
                return GetBytesFromData(length);
            }

            Array.Resize(ref dataFromClient, 0);
            return GetBytesFromData(length);
        }

        private byte[] GetBytesFromData(int length)
        {
            Task<int> bytesReceived = stream.ReadAsync(data).AsTask();
            Task toContinue = bytesReceived.ContinueWith(CopyBytesAndResize, length);
            toContinue.Wait();

            if (dataFromClient.Length >= length)
                return TreatDataOverflow(length);
            return dataFromClient;
        }

        private void CopyBytesAndResize(Task<int> bytesReceived, object length)
        {
            if (bytesReceived.Result == 0 || (int)length == 0)
                throw new IOException();

            int index = dataFromClient.Length;
            Array.Resize(ref dataFromClient, dataFromClient.Length + bytesReceived.Result);
            Array.Copy(data, 0, dataFromClient, index, bytesReceived.Result);

            while (dataFromClient.Length < (int)length)
                GetBytesFromData((int)length);
        }

        private byte[] TreatDataOverflow(int length)
        {
            Array.Resize(ref remaining, dataFromClient.Length - length);
            Array.Copy(dataFromClient, length, remaining, 0, remaining.Length);
            Array.Resize(ref dataFromClient, length);
            return dataFromClient;
        }

        private bool CheckRemaining()
            => (Object.Equals(remaining, null) || remaining.Length == 0) ? false : true;
    }
}