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
            Task.Run(() => HandleClient(client));
        }

        private void HandleClient(Participant client)
        {
            try
            {
                client.ReceiveUsername();
                SendMessageToAllClients(new Message($"{client.Username} is now online !"));
                while (client.isConnected)
                {
                    Message message = client.RetrieveMessage();
                    if (message.Equals(new Message($"{client.Username} is now offline !")))
                        client.isConnected = false;
                    SendMessageToAllClients(message);
                }
                Leave(client, false);
            }
            catch (IOException)
            {
                Leave(client, true);
            }
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