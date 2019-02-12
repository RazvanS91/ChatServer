using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    public class ChatRoom
    {
        private List<Participant> clients = new List<Participant>();

        public async Task Join(Participant client)
        {
            clients.Add(client);
            await client.ReceiveUsername();
            await SendMessageToAllClients(new Message($"{client.Username} is now online !"));
            while (client.isConnected)
            {
                Message message = await client.RetrieveMessage();
                if (message.Equals(new Message($"{client.Username} is now offline !")))
                    Leave(client, false);
                await SendMessageToAllClients(message);
            }
        }

        public void Leave(Participant client, bool hasLostConnection)
        {
            clients.Remove(client);
            client.Disconnect();
            if (hasLostConnection)
                SendMessageToAllClients(new Message($"{client.Username} has lost connection !"));
        }

        public async Task SendMessageToAllClients(Message message)
        {
            foreach (Participant client in clients)
            {
                Console.WriteLine($"Message sent to {client.Username} : {Encoding.ASCII.GetString(message.ToByteArray())}");
                await client.Send(message);
            }
        }
    }
}
