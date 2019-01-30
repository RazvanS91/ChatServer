using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace ChatServer
{
    public class ChatRoom
    {
        private List<Participant> clients = new List<Participant>();

        public void Join(Participant client)
        {
            clients.Add(client);
            HandleClient(client);
        }

        private Task HandleClient(Participant client)
        {
            return client.ReceiveUsername().ContinueWith(uName =>
            {
                SendMessageToAllClients(new Message($"{client.Username} is now online !"));
                while (client.isConnected)
                {
                    Message message = client.RetrieveMessage().Result;
                    if (message.Equals(new Message($"{client.Username} is now offline !")))
                        client.isConnected = false;
                    SendMessageToAllClients(message);
                }
            Leave(client, false);
            });
        }

        public void Leave(Participant client, bool hasLostConnection)
        {
            client.isConnected = false;
            clients.Remove(client);
            client.Disconnect();
            if (hasLostConnection)
                SendMessageToAllClients(new Message($"{client.Username} lost connection !"));
        }

        public void SendMessageToAllClients(Message message)
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