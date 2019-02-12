using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Participant
    {
        private StreamReader sReader;
        private TcpClient client;
        public string Username { get; private set; }
        internal bool isConnected;

        public Participant(TcpClient client)
        {
            this.client = client;
            sReader = new StreamReader(client.GetStream());
        }

        public async Task ReceiveUsername()
        {
            var bytes = await GetDataFromClient();
            Username = Encoding.ASCII.GetString(bytes);
            isConnected = true;
        }

        public async Task<Message> RetrieveMessage()
        {
            var bytes = await GetDataFromClient();
            if (bytes.Length == 0)
                return new Message($"{Username} has lost connection !");
            var message = new Message(bytes);
            if (message.Equals(new Message($"{Username} is now offline !")))
            {
                isConnected = false;
                return message;
            }
            return new Message(Username, message);
        }

        private async Task<byte[]> GetDataFromClient()
        {
            var length = await sReader.ReadShort();
            var data =  await sReader.GetData(length);
            return data;
        }

        public async Task Send(Message message)
        {
            await sReader.WriteShort(message.Length);
            await sReader.WriteData(message.ToByteArray());
        }

        public void Disconnect()
        {
            sReader.Close();
            client.Close();
        }
    }
}