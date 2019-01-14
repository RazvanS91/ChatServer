using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace ChatServer
{
    public class Server
    {
        private readonly TcpListener server = new TcpListener(IPAddress.Parse("192.168.1.105"), 12345);
        private readonly ChatRoom chatRoom = new ChatRoom();

        public void Start()
        {
            server.Start();
            while (true)
            {
                AcceptClient();
            }
        }

        private void AcceptClient()
        {
            var participant = new Participant(server.AcceptTcpClient());
            chatRoom.Join(participant);
        }
    }
}