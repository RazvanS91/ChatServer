using System;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server();
            server.Start();
        }
    }
}
