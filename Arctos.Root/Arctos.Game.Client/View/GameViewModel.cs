using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Arctos.Game.GUIClient;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;

namespace Arctos.View
{
    public class GameViewModel : PropertyChangedBase
    {
        public GameViewModel()
        {
            // this.GetExampleGame();
            this.GameConnected = false;

            worker.WorkerSupportsCancellation = true;
            worker.DoWork += ReceiveEvents;
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
                        this.ConnectToGame("172.22.25.74");
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
        /// Connect to GameServer to get Game configuration and current game state
        /// </summary>
        private void ConnectToGame(string gameServer)
        {
            try
            {
                this.GameClient = new GameTcpClient(gameServer);
                if (this.GameClient.Connected)
                {
                    // Request gui for username
                    this.GameClient.Send(new GameEvent<String>(GameEventType.GuiRequest, "ninos"));

                    GameEvent<dynamic> gameEvent;
                    do
                    {
                        gameEvent = this.GameClient.Receive();
                    } while (gameEvent != null && gameEvent.EventGameEventType != GameEventType.GuiJoined);

                    if (gameEvent == null) return;

                    var gameArea = (GameArea) gameEvent.Data;
                    if (gameArea != null)
                    {
                        this.Game = new GuiGameArea(gameArea)
                        {
                            AreaWidth = 1600,
                            AreaHeight = 850
                        };
                    }


                    this.GameConnected = true;
                    worker.RunWorkerAsync();
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

        /// <summary>
        /// Wait for incoming events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private void ReceiveEvents(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (true)
            {
                var receivedEvent = this.GameClient.Receive();

                if (receivedEvent != null)
                {
                    switch (receivedEvent.EventGameEventType)
                    {
                        case GameEventType.AreaUpdate:
                        {
                            var receivedAreaUpdate = receivedEvent.Data as Area;
                            if (receivedAreaUpdate != null)
                            {
                                var foundArea =
                                    this.Game.AreaList.FirstOrDefault(x => x.AreaId.Equals(receivedAreaUpdate.AreaId));
                                if (foundArea != null)
                                    foundArea.IsActive = true;
                            }
                        }
                            break;
                    }
                }

                if (worker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    this.GameConnected = false;
                    return;
                }
            }
        }

        /// <summary>
        /// Generate a example game field
        /// </summary>
        private void GetExampleGame()
        {
            var areas = new ObservableCollection<GuiArea>();
            for (var rows = 0; rows < 4; rows++)
            {
                for (var cols = 0; cols < 4; cols++)
                {
                    areas.Add(new GuiArea
                    {
                        AreaId = "",
                        Column = cols,
                        Row = rows,
                        IsActive = false
                    });
                }
            }

            var game = new GuiGameArea
            {
                AreaWidth = 1600,
                AreaHeight = 850,
                AreaList = areas
            };

            this.Game = game;
        }

        #region Properties

        private GuiGameArea _game;

        public GuiGameArea Game
        {
            get { return _game; }
            set
            {
                _game = value;
                OnPropertyChanged();
            }
        }

        private GameTcpClient GameClient { get; set; }
        private bool GameConnected { get; set; }

        private readonly BackgroundWorker worker = new BackgroundWorker();

        #endregion
    }
}