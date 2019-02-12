using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ChatServer
{
    class StreamReader
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

        public async Task<short> ReadShort()
        {
            var data = await GetData(2);
            if (data.Length == 0)
                return 0;
            else
                return BitConverter.ToInt16(data);
        }

        public async Task<string> ReadString(short size)
        {
            var data = await GetData(size);
            return Encoding.ASCII.GetString(data);
        }

        public async Task WriteShort(short value)
        {
            await WriteData(BitConverter.GetBytes(value));
        }

        public async Task WriteData(byte[] value)
        {
            await stream.WriteAsync(value);
        }

        public void Close()
        {
            stream.Close();
        }

        public async Task<byte[]> GetData(int length)
        {
            if (CheckRemaining())
            {
                dataFromClient = remaining;
                if (dataFromClient.Length < length)
                    return await GetBytesFromData(length);

                TreatDataOverflow(dataFromClient.Length - length);
                return dataFromClient;
            }
            Array.Resize(ref dataFromClient, 0);
            return await GetBytesFromData(length);
        }

        private async Task<byte[]> GetBytesFromData(int length)
        {
            do
            {
                var read = await stream.ReadAsync(data);
                if (read == 0)
                    return new byte[0];
                ResizeAndCopy(read);
            }
            while (dataFromClient.Length < length);

            TreatDataOverflow(dataFromClient.Length - length);
            return dataFromClient;
        }

        private void TreatDataOverflow(int extraLength)
        {
            int index = dataFromClient.Length - extraLength;
            Array.Resize(ref remaining, extraLength);
            Array.Copy(dataFromClient, index, remaining, 0, extraLength);
            Array.Resize(ref dataFromClient, index);
        }

        private void ResizeAndCopy(int bytesRead)
        {
            int index = dataFromClient.Length;
            Array.Resize(ref dataFromClient, dataFromClient.Length + bytesRead);
            Array.Copy(data, 0, dataFromClient, index, bytesRead);
        }

        private bool CheckRemaining() => !Object.Equals(remaining, null) ? true : false;
    }
}