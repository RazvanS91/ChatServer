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

        public void Join(ParticipantS client)
        {
            clients.Add(client);
            HandleClient(client);
        }

        public void Add(ParticipantS client)
        {
            clients.Add(client);
        }

        private void HandleClient(ParticipantS client)
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

        public void Leave(ParticipantS client, bool hasLostConnection)
        {
            client.isConnected = false;
            clients.Remove(client);
            client.Disconnect();
            if (hasLostConnection)
                SendMessageToAllClients(new Message($"{client.Username} lost connection !"));
        }

        public void SendMessageToAllClients(Message message)
        {
            List<ParticipantS> clientsToRemove = new List<ParticipantS>();
            foreach(var client in clients)
            {
                try
                {
                    client.Send(message);
                    client.Message = Encoding.ASCII.GetString(message.ToByteArray());
                    Console.WriteLine($"Message succesfully sent at {DateTime.Now} to {client.Username} : {Encoding.ASCII.GetString(message.ToByteArray())}");
                }
                catch(ObjectDisposedException)
                {
                    clientsToRemove.Add(client);
                }
            }

            foreach (var client in clientsToRemove)
                Leave(client, true);
        }
    }
}