using System;
using System.Windows;
using Arctos.Game.GUIClient;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using Arctos.View.Utilities;

namespace Arctos.View
{
    public class ConnectViewModel : PropertyChangedBase
    {
        #region Properties

        private bool _closeTrigger;
        public bool CloseTrigger
        {
            get { return this._closeTrigger; }
            set
            {
                _closeTrigger = value;
                this.CurrentGameView = null;
                OnPropertyChanged();
            }
        }

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

        private bool _showGameInformation;
        public bool ShowGameInformation
        {
            get { return _showGameInformation; }
            set
            {
                _showGameInformation = value;
                OnPropertyChanged();
            }
        }

        private string _gameInformation;
        public string GameInformation
        {
            get { return _gameInformation; }
            set
            {
                _gameInformation = value;
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
            this.GameServer = "172.22.25.74";
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
                    // Request GUI for username
                    case "GuiRequest":
                    {
                        if (string.IsNullOrEmpty(this.PlayerName))
                        {
                            this.ShowInformationOverlay("Please set your Player name");
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
                this.ShowInformationOverlay("Error: " + ex.Message);
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
                    this.ShowInformationOverlay("Could not connect to GameServer");
                }
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay("Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Received Event from GameServer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameClientOnReceivedDataEvent(object sender, ReceivedEventArgs args)
        {
            try
            {
                var gameEvent = args.Data as GameEvent;
                if (gameEvent != null && gameEvent.EventType == GameEvent.Type.GuiJoined)
                {
                    var gameArea = (Game.Middleware.Logic.Model.Model.Game) gameEvent.Data;

                    if (gameArea != null)
                    {
                        this.GameConnected = true;

                        this.CurrentGameView = new GameView {DataContext = new GameViewModel(this.GameClient, gameArea)};
                        this.CurrentGameView.Show();
                    }
                    else
                    {
                        this.ShowInformationOverlay("Did not receive any new Games! Please try again.");
                    }

                    this.GameClient.ReceivedDataEvent -= GameClientOnReceivedDataEvent;
                }
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay("Error while waiting for GameServer: " + ex.Message);
            }
        }

        #region ViewHelper

        /// <summary>
        /// Show a message overlay
        /// </summary>
        /// <param name="message"></param>
        private void ShowInformationOverlay(string message)
        {
            this.GameInformation = message;
            this.ShowGameInformation = true;

            ViewHelper.Wait(2);

            this.ShowGameInformation = false;
            this.GameInformation = "";
        }
        
        #endregion
    }
}