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
        private byte[] dataFromClient = new byte[0];
        private byte[] remaining;
        private readonly Stream stream;
        private TaskCompletionSource<byte[]> tcs;

        public StreamReader(Stream stream)
        {
            this.stream = stream;
            data = new byte[12];
        }

        public Task<short> ReadShort()
        {
            return GetData(2).ContinueWith(x => BitConverter.ToInt16(x.Result));
        }

        public Task<string> ReadString(short size)
        {
            return GetData(size).ContinueWith(x => Encoding.ASCII.GetString(x.Result));
        }

        public Task WriteShort(short value)
        {
            return WriteData(BitConverter.GetBytes(value));
        }

        public Task WriteData(byte[] value)
        {
            return stream.WriteAsync(value).AsTask();
        }

        public void Close()
        {
            stream.Close();
        }

        public Task<byte[]> GetData(int length)
        {
            if (CheckRemaining())
            {
                tcs = new TaskCompletionSource<byte[]>();
                dataFromClient = remaining;
                if (dataFromClient.Length >= length)
                {
                    TreatDataOverflow(dataFromClient.Length - length);
                    tcs.SetResult(dataFromClient);
                    return tcs.Task;
                }
                return GetRemainingData(length - dataFromClient.Length);
            }
            Array.Resize(ref dataFromClient, 0);

            return stream.ReadAsync(data).AsTask().ContinueWith(bytesRead =>
            {
                ResizeAndCopy(bytesRead.Result);
                if (dataFromClient.Length < length)
                    return GetRemainingData(length - bytesRead.Result).Result;
                TreatDataOverflow(dataFromClient.Length - length);
                return dataFromClient;
            });
        }

        private Task<byte[]> GetRemainingData(int remainingLength)
        {
            Console.WriteLine($"GetRemainingData >> {remainingLength}");
            var tcs = new TaskCompletionSource<byte[]>();

            stream.ReadAsync(data).AsTask().ContinueWith(taskRes =>
            {
                Console.WriteLine($"Read {taskRes.Result} from {remainingLength}");
                ResizeAndCopy(taskRes.Result);
                if (taskRes.Result >= remainingLength)
                {
                    Console.WriteLine($"Done reading. Setting result");
                    TreatDataOverflow(taskRes.Result - remainingLength);
                    tcs.SetResult(dataFromClient);
                    return;
                }
                Console.WriteLine($"Not done. Calling getRemainingData again...");
                GetRemainingData(remainingLength - taskRes.Result);
                Console.WriteLine($"Call complete");
            });
            Console.WriteLine("GetRemainingData << ");
            return tcs.Task;
        }

        private void ResizeAndCopy(int bytesRead)
        {
            int index = dataFromClient.Length;
            Array.Resize(ref dataFromClient, dataFromClient.Length + bytesRead);
            Array.Copy(data, 0, dataFromClient, index, bytesRead);
        }

        private void TreatDataOverflow(int extraLength)
        {
            int index = dataFromClient.Length - extraLength;
            Array.Resize(ref remaining, extraLength);
            Array.Copy(dataFromClient, index, remaining, 0, extraLength);
            Array.Resize(ref dataFromClient, index);
        }

        private bool CheckRemaining()
            => (Object.Equals(remaining, null) || remaining.Length == 0) ? false : true;
    }
}