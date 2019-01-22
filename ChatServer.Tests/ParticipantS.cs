using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
    public class ParticipantS
    {
        private StreamReader sReader;
        private Stream stream;
        internal bool isConnected;

        public string Username { get; private set; }
        public string Message { get; set; }

        public ParticipantS(Stream stream)
        {
            this.stream = stream;
            sReader = new StreamReader(stream);
        }

        public void ReceiveUsername()
        {
            Username = Encoding.ASCII.GetString(GetDataFromClient());
            isConnected = true;
        }

        public Message RetrieveMessage()
        {
            string dataFromClient = Encoding.ASCII.GetString(GetDataFromClient());
            if (dataFromClient == $"{Username} is now offline !")
            {
                Message = dataFromClient;
                return new Message(dataFromClient);
            }
            Message message = new Message(dataFromClient);
            Message = dataFromClient;
            return new Message(Username, message);
        }

        private byte[] GetDataFromClient()
        {
            return sReader.GetData(sReader.ReadShort());
        }

        public void Send(Message message)
        {
            sReader.WriteShort(message.Length);
            sReader.WriteData(message.ToByteArray());
        }

        public void Disconnect()
        {
            sReader.Close();
        }
    }
}