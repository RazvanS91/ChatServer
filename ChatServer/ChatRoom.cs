using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    public class ChatRoom
    {
        private List<Participant> clients = new List<Participant>();

        public void Join(Participant client)
        {
            clients.Add(client);
            client.ReceiveUsername();
            while (true)
            {
                Message message = client.RetrieveMessage();
                SendMessageToAllClients(message);
            }
        }

        public void Leave(Participant client)
        {
            clients.Remove(client);
            client.Disconnect();
        }

        public void SendMessageToAllClients(Message message)
        {
            foreach (Participant client in clients)
                client.Send(message);
        }
    }
}
