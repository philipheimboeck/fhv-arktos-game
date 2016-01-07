using ArctosGameServer.Controller;
using ArctosGameServer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArctosGameServer
{
    class Server
    {
        static GameController game;
        static GameTcpServer server; 

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);

            Console.WriteLine("Starting server...");

            // Instantiate components
            server = new GameTcpServer();
            game = new GameController();

            // Add listeners
            server.Subscribe(game);
        
            // Start the TCP Service
            server.StartService();

            // Start the game
            game.loop();
        }

        static void ProcessExit(object o, EventArgs args)
        {
            if (server != null)
            {
                server.CloseConnections();
            }
            if (game != null)
            {
                game.ShutdownRequested = true; 
            }
        }
    }
}
