using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Serialization;
using Arctos.Game.Model;
using ArctosGameServer.Controller;
using ArctosGameServer.Service;
using ArctosGameServer.ViewModel;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace ArctosGameServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GameController _game;
        private GameTcpServer _server;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // Check for path parameter
            string path = null;
            if (e.Args.Length > 0)
            {
                path = e.Args[0];

                if (!File.Exists(path))
                {
                    path = null;
                }
            }

            // Open File dialog when no path is set
            if(path == null)
            {
                var dialog = new OpenFileDialog();
                dialog.InitialDirectory = Environment.CurrentDirectory;
                dialog.Filter = "Map Files|*.map";

                var result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                {
                    Shutdown();
                    return;
                }

                path = dialog.FileName;
            }

            GameConfiguration entity = null;
            try
            {
                // Deserialize the entity
                var serializer = new XmlSerializer(typeof (GameConfiguration));
                using (var stream = new StreamReader(path))
                {
                    entity = serializer.Deserialize(stream) as GameConfiguration;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                Shutdown();
                return;
            }

            // Start the components
            StartComponents(entity);

            // Create a viewmodel
            var vm = new GameViewModel(_game);

            // Initialize window
            var window = new Server
            {
                DataContext = vm
            };
            window.Show();
            
        }

        private void StartComponents(GameConfiguration configuration)
        {
            // Instantiate components
            _server = new GameTcpServer();
            _game = new GameController(_server, configuration);

            // Add listeners
            _server.Subscribe(_game);

            // Start the TCP Service
            _server.StartService();

            // Create and start the DiscoveryService
            var discovery = new DiscoveryService();
            discovery.Start();

            // Start the game
            var worker = new BackgroundWorker();
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