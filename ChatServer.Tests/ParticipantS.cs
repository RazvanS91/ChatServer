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
        private Action<Message> broadcast;

        public string Username { get; private set; }
        public string Message { get; set; }

        public ParticipantS(Stream stream)
        {
            this.stream = stream;
            sReader = new StreamReader(stream);
        }

        public void StartConversation(Action<Message> callback)
        {
            broadcast = callback;
            GetDataFromClient(b =>
            {
                Username = Encoding.ASCII.GetString(b);
                isConnected = true;
                broadcast(new Message($"{Username} is now online !"));
                RecursivelyGetMessages();
            });
        }

        private void RecursivelyGetMessages()
        {
            RetrieveMessage(m =>
            {
                broadcast(m);
                if (isConnected)
                    RecursivelyGetMessages();
            });
        }

        public void RetrieveMessage(Action<Message> callback)
        {
            GetDataFromClient(r =>
            {
                if (r.Length == 0)
                    isConnected = false;
                else
                {
                    var data = Encoding.ASCII.GetString(r);
                    Message = data;
                    Message message = new Message(data);
                    if (data == $"{Username} is now offline !")
                    {
                        isConnected = false;
                        callback(message);
                    }
                    else
                        callback(new Message(Username, message));
                }
            });
        }

        private void GetDataFromClient(Action<byte[]> callback)
        {
            sReader.ReadShort(length =>
            {
                if (length == 0)
                    callback(new byte[0]);
                sReader.GetData(length, r => callback(r));
            });
        }

        public void Send(Message message)
        {
            sReader.WriteShort(message.Length);
            sReader.WriteData(message.ToByteArray());
        }

        public void Disconnect()
        {
            sReader.Close();
            stream.Close();
        }
    }
}