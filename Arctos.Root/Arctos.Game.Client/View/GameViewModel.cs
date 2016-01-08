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
        #region Properties

        private GuiGameArea _game;
        public GuiGameArea Game
        {
            get { return _game; }
            set { _game = value; OnPropertyChanged(); }
        }

        private GameTcpClient GameClient { get; set; }
        private bool GameConnected { get; set; }

        private readonly BackgroundWorker worker = new BackgroundWorker();

        #endregion

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
                    this.GameClient.Send(new GameEvent(GameEvent.Type.GuiRequest, "ninos"));

                    GameEvent gameEvent;
                    do {
                        gameEvent = this.GameClient.Receive();
                    } while (gameEvent != null && gameEvent.EventType != GameEvent.Type.GuiJoined);

                    if (gameEvent == null) return;

                    GameArea gameArea = (GameArea) gameEvent.Data;
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

        public GameViewModel()
        {
           // this.GetExampleGame();
            this.GameConnected = false;

            worker.WorkerSupportsCancellation = true;
            worker.DoWork += ReceiveEvents;
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
                GameEvent receivedEvent = this.GameClient.Receive();

                if (receivedEvent != null)
                {
                    switch (receivedEvent.EventType)
                    {
                        case GameEvent.Type.AreaUpdate:
                        {
                            Area receivedAreaUpdate = receivedEvent.Data as Area;
                            if (receivedAreaUpdate != null)
                            {
                                var foundArea = this.Game.AreaList.FirstOrDefault(x => x.AreaID.Equals(receivedAreaUpdate.AreaId));
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
            ObservableCollection<GuiArea> areas = new ObservableCollection<GuiArea>();
            for (int rows = 0; rows < 4; rows++)
            {
                for (int cols = 0; cols < 4; cols++)
                {
                    areas.Add(new GuiArea
                    {
                        AreaID = "",
                        Column = cols,
                        Row = rows,
                        IsActive = false
                    });
                }
            }

            GuiGameArea game = new GuiGameArea
            {
                AreaWidth = 1600,
                AreaHeight = 850,
                AreaList = areas
            };

            this.Game = game;
        }

    }
}
