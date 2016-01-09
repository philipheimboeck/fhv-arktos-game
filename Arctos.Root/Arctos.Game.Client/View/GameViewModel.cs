using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Arctos.Game.GUIClient;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;

namespace Arctos.View
{
    /// <summary>
    /// GUI View Model
    /// </summary>
    public class GameViewModel : PropertyChangedBase
    {
        #region Properties

        private bool _showGameInformation = false;
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

        private GuiGameArea _guiGameInstance;
        public GuiGameArea GUIGameInstance
        {
            get { return _guiGameInstance; }
            set
            {
                _guiGameInstance = value;
                OnPropertyChanged();
            }
        }

        private GameArea GameArea { get; set; }

        private GameTcpClient GameClient { get; set; }
        private readonly BackgroundWorker eventBackgroundWorker = new BackgroundWorker();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="area"></param>
        public GameViewModel(GameTcpClient client, GameArea area)
        {
            this.GUIGameInstance = new GuiGameArea(area)
            {
                AreaWidth = 800,
                AreaHeight = 600
            };

            this.GameArea = area;
            this.GameClient = client;
            this.GameClient.ReceivedDataEvent += GameClientOnReceivedEvent;

            InitializeGameViewModel();
        }

        /// <summary>
        /// Initialize GameView Model
        /// </summary>
        private void InitializeGameViewModel()
        {
            eventBackgroundWorker.WorkerSupportsCancellation = true;
            eventBackgroundWorker.DoWork += WaitForReceivingData;
            eventBackgroundWorker.RunWorkerAsync();
        }

        /// <summary>
        /// Wait for incoming events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private void WaitForReceivingData(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (true)
            {
                try
                {
                    this.GameClient.Receive();

                    if (eventBackgroundWorker.CancellationPending)
                    {
                        doWorkEventArgs.Cancel = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// Received a new GameEvent
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receivedEventArgs"></param>
        private void GameClientOnReceivedEvent(object sender, ReceivedEventArgs receivedEventArgs)
        {
            var receivedEvent = receivedEventArgs.Data as GameEvent;
            if (receivedEvent == null) return;
            switch (receivedEvent.EventType)
            {
                // Area Update
                case GameEvent.Type.AreaUpdate:
                {
                    var receivedAreaUpdate = receivedEvent.Data as Area;
                    if (receivedAreaUpdate != null)
                    {
                        GuiArea foundArea =
                            this.GUIGameInstance.AreaList.FirstOrDefault(
                                x => x.AreaId.Equals(receivedAreaUpdate.AreaId));
                        if (foundArea != null)
                            foundArea.Status = receivedAreaUpdate.Status;
                    }
                }
                break;

                // Game Ready, show the path
                case GameEvent.Type.GameReady:
                {
                    var receivedAreaUpdate = receivedEvent.Data as Path;
                    if (receivedAreaUpdate == null)
                    {
                        // game not ready, show as message
                        this.ShowInformationOverlay("Game is not ready. Please Wait.");
                    }
                    else
                    {
                        this.ShowInformationOverlay("READY ....");

                        this.GameArea.setPath(receivedAreaUpdate.Waypoints.Select(x => new Tuple<int, int>(x.Item1, x.Item2)).ToList());

                        // Show Path step by step
                        foreach (Area areaPath in this.GameArea.Path)
                        {
                            this.GUIGameInstance.AreaList.FirstOrDefault(area => area.AreaId.Equals(areaPath.AreaId)).Status = Area.AreaStatus.CorrectlyPassed;
                            Wait(1);
                        }
                    }
                }
                break;

                // Set all Areas back
                case GameEvent.Type.GameStart:
                {
                    this.GUIGameInstance.AreaList.Select(i =>
                    {
                        i.Status = Area.AreaStatus.None;
                        return i;
                    }).ToList();
                }
                break;
            }
        }

        #region View Helper

        /// <summary>
        /// Show a message overlay
        /// </summary>
        /// <param name="message"></param>
        private void ShowInformationOverlay(string message)
        {
            this.GameInformation = message;
            this.ShowGameInformation = true;

            Wait(2);

            this.ShowGameInformation = false;
            this.GameInformation = "";
        }

        /// <summary>
        /// GUI Wait for x seconds
        /// </summary>
        /// <param name="seconds"></param>
        private void Wait(double seconds)
        {
            var frame = new DispatcherFrame();
            new Thread((ThreadStart)(() =>
            {
                Thread.Sleep(TimeSpan.FromSeconds(seconds));
                frame.Continue = false;
            })).Start();
            Dispatcher.PushFrame(frame);
        }

        #endregion
    }
}