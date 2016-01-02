using ArctosGameServer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArctosGameServer
{
    class Server
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            var server = new GameTcpServer();
        
            // Start the TCP Service
            server.StartService();
        }
    }
}
