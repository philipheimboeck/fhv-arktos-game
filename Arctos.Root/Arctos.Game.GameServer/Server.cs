using System;
using ArctosGameServer.Controller;
using ArctosGameServer.Service;

namespace ArctosGameServer
{
    internal class Server
    {
        private static GameController game;
        private static GameTcpServer server;

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ProcessExit);

            Console.WriteLine("Starting server...");

            // Instantiate components
            server = new GameTcpServer();
            game = new GameController(server);

            // Add listeners
            server.Subscribe(game);

            // Start the TCP Service
            server.StartService();

            // Start the game
            game.Loop();
        }

        private static void ProcessExit(object o, EventArgs args)
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