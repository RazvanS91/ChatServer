using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;

namespace ChatServer
{
    public class Participant
    {
        private StreamReader sReader;
        private TcpClient client;
        private string username;

        public Participant(TcpClient client)
        {
            this.client = client;
            sReader = new StreamReader(client.GetStream());
        }

        public void ReceiveUsername()
        {
            username = Encoding.ASCII.GetString(GetDataFromClient());
        }

        public Message RetrieveMessage()
        {
            string dataFromClient = Encoding.ASCII.GetString(GetDataFromClient());
            return new Message(dataFromClient);
        }

        private byte[] GetDataFromClient()
        {
            return sReader.GetData(sReader.ReadShort());
        }

        public void Send(Message message)
        {
            var msg = new Message(username, message);
            sReader.WriteShort(msg.Length);
            sReader.WriteData(msg.ToByteArray());
        }

        public void Disconnect()
        {
            sReader.Close();
            client.Close();
        }
    }
}