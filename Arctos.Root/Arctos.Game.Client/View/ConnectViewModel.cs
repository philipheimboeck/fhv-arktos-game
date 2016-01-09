using System;
using System.ComponentModel;
using System.Windows;
using Arctos.Game.GUIClient;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;

namespace Arctos.View
{
    public class ConnectViewModel : PropertyChangedBase
    {
        #region Properties

        private const string CONNECT = "Connect";
        private const string DISCONNECT = "Disconnect";

        private string _buttonConnect = CONNECT;
        public string ButtonConnect
        {
            get { return _buttonConnect; }
            set
            {
                _buttonConnect = value;
                OnPropertyChanged();
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnPropertyChanged();
            }
        }

        private string _gameServer;
        public string GameServer
        {
            get { return _gameServer; }
            set
            {
                _gameServer = value;
                OnPropertyChanged();
            }
        }

        private bool GameConnected { get; set; }
        private GameTcpClient GameClient { get; set; }

        private Window CurrentGameView { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ConnectViewModel()
        {
            GameServer = "172.22.25.74";
        }

        /// <summary>
        /// Execute Command from View
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            try
            {
                switch (parameter.ToString())
                {
                    case "GuiRequest":
                    {
                        if (string.IsNullOrEmpty(this.PlayerName))
                        {
                            MessageBox.Show("Please set your Player name");
                        }
                        else 
                        { 
                            this.ConnectToGame(this.GameServer);
                        }
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Connect to GameServer to get GUIGameInstance configuration and current game state
        /// </summary>
        private void ConnectToGame(string gameServer)
        {
            try
            {
                this.GameClient = new GameTcpClient(gameServer);
                if (this.GameClient.Connected)
                {
                    // Request gui for username
                    this.GameClient.Send(new GameEvent(GameEvent.Type.GuiRequest, this.PlayerName));

                    this.GameClient.ReceivedDataEvent += GameClientOnReceivedDataEvent;
                    this.GameClient.Receive();
                }
                else
                {
                    this.GameConnected = false;
                    MessageBox.Show("Could not connect to GameServer");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GameClientOnReceivedDataEvent(object sender, ReceivedEventArgs args)
        {
            var gameEvent = args.Data as GameEvent;
            var gameArea = (GameArea)gameEvent.Data;

            if (gameArea != null)
            {
                this.GameConnected = true;

                this.CurrentGameView = new GameView { DataContext = new GameViewModel(this.GameClient, gameArea) };
                this.CurrentGameView.Show();
            }
            else
            {
                MessageBox.Show("Did not receive any new Games! Please try again.");
            }

            this.GameClient.ReceivedDataEvent -= GameClientOnReceivedDataEvent;
        }
    }
}