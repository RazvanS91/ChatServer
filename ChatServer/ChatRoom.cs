using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatRoom
    {
        private List<Participant> clients = new List<Participant>();

        public void Join(Participant client)
        {
            clients.Add(client);
            client.StartConversation(m =>
            {
                if(m.Equals(new Message($"{client.Username} is now offline !")))
                {
                    Leave(client, false);
                    SendMessageToAllClients(m);
                }
                else if (m.Equals(new Message($"{client.Username} has lost connection !")))
                    Leave(client, true);
                else SendMessageToAllClients(m);
            });
        }

        public void Leave(Participant client, bool hasLostConnection)
        {
            clients.Remove(client);
            client.Disconnect();
            if (hasLostConnection)
                SendMessageToAllClients(new Message($"{client.Username} lost connection !"));
        }

        private void SendMessageToAllClients(Message message)
        {
            List<Participant> clientsToRemove = new List<Participant>();

            foreach (var client in clients)
            {
                try
                {
                    client.Send(message);
                    Console.WriteLine($"Message sent to {client.Username} : {Encoding.ASCII.GetString(message.ToByteArray())}");
                }
                catch (ObjectDisposedException)
                {
                    clientsToRemove.Add(client);
                }
            }
            foreach (var client in clientsToRemove)
                Leave(client, true);
        }
    }
}