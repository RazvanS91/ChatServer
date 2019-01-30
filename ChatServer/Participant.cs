using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class Participant
    {
        private StreamReader sReader;
        private TcpClient client;
        internal bool isConnected;

        public string Username { get; private set; }

        public Participant(TcpClient client)
        {
            this.client = client;
            sReader = new StreamReader(client.GetStream());
        }

        public Task ReceiveUsername()
        {
            return sReader.ReadShort().ContinueWith(x =>
            {
                return sReader.ReadString(x.Result).Result;
            })
            .ContinueWith(y =>
                {
                    isConnected = true;
                    Username = y.Result;
                });
        }

        public Task<Message> RetrieveMessage()
        {
            return sReader.ReadShort().ContinueWith(x =>
            {
                return sReader.ReadString(x.Result).Result;
            })
            .ContinueWith(y =>
                {
                    string dataFromClient = y.Result;
                    if (dataFromClient == $"{Username} is now offline !")
                        return new Message(dataFromClient);
                    Message message = new Message(dataFromClient);
                    return new Message(Username, message);
                });
        }

        public Task Send(Message message)
        {
            return sReader.WriteShort(message.Length).ContinueWith(x => sReader.WriteData(message.ToByteArray()));
        }

        public void Disconnect()
        {
            sReader.Close();
            client.Close();
        }
    }
}