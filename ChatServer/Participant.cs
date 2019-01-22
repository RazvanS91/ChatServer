﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;

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

        public void ReceiveUsername()
        {
            Username = Encoding.ASCII.GetString(GetDataFromClient());
            isConnected = true;
        }

        public Message RetrieveMessage()
        {

            string dataFromClient = Encoding.ASCII.GetString(GetDataFromClient());
            if (dataFromClient == $"{Username} is now offline !")
                return new Message(dataFromClient);
            Message message = new Message(dataFromClient);
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
            client.Close();
        }
    }
}