using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatRoomS
    {
        private List<ParticipantS> clients = new List<ParticipantS>();

        public void Add(ParticipantS client)
        {
            clients.Add(client);
        }

        public void Join(ParticipantS client)
        {
            clients.Add(client);
            client.StartConversation(m =>
            {
                if (m.Equals(new Message($"{client.Username} is now offline !")))
                {
                    Leave(client, false);
                    SendMessageToAllClients(m);
                }
                else if (m.Equals(new Message($"{client.Username} has lost connection !")))
                    Leave(client, true);
                else SendMessageToAllClients(m);
            });
        }

        public void Leave(ParticipantS client, bool hasLostConnection)
        {
            clients.Remove(client);
            client.Disconnect();
            if (hasLostConnection)
                SendMessageToAllClients(new Message($"{client.Username} lost connection !"));
        }

        public void SendMessageToAllClients(Message message)
        {
            List<ParticipantS> clientsToRemove = new List<ParticipantS>();

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