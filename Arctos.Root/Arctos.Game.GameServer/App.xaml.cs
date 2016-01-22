using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ArctosGameServer.Controller;
using ArctosGameServer.Service;
using ArctosGameServer.ViewModel;

namespace ArctosGameServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GameTcpServer _server;
        private GameController _game;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Start the components
            StartComponents();

            // Create a viewmodel
            var vm = new GameViewModel(_game);

            // Initialize window
            var window = new Server
            {
                DataContext = vm
            };
            window.Show();
        }
       
        private void StartComponents()
        {
            // Instantiate components
            _server = new GameTcpServer();
            _game = new GameController(_server);

            // Add listeners
            _server.Subscribe(_game);

            // Start the TCP Service
            _server.StartService();

            // Create and start the DiscoveryService
            var discovery = new DiscoveryService();
            discovery.Start();

            // Start the game
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => _game.Loop();
            worker.RunWorkerAsync();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            // Close the game
            if (_server != null) _server.CloseConnections();

            if (_game != null)
            {
                _game.ShutdownRequested = true;
            }
        }
    }
}