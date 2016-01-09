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
        /// Show a message overlay
        /// </summary>
        /// <param name="message"></param>
        public void ShowInformationOverlay(string message)
        {
            this.GameInformation = message;
            this.ShowGameInformation = true;
            Wait(2);
            this.ShowGameInformation = false;
            this.GameInformation = "";
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gameArea"></param>
        public GameViewModel(GameTcpClient client)
        {
            this.GameClient = client;

            InitializeGameViewModel();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public GameViewModel()
        {
            InitializeGameViewModel();
        }

        /// <summary>
        /// Initialize GameView Model
        /// </summary>
        private void InitializeGameViewModel()
        {
            eventBackgroundWorker.WorkerSupportsCancellation = true;
            eventBackgroundWorker.DoWork += ReceiveEvents;
            eventBackgroundWorker.RunWorkerAsync();
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
                try
                {
                    var receivedEvent = this.GameClient.Receive();

                    if (receivedEvent != null)
                    {
                        switch (receivedEvent.EventType)
                        {
                            // Area Update
                            case GameEvent.Type.AreaUpdate:
                            {
                                var receivedAreaUpdate = receivedEvent.Data as Area;
                                if (receivedAreaUpdate != null)
                                {
                                    var foundArea =
                                        this.GUIGameInstance.AreaList.FirstOrDefault(
                                            x => x.AreaId.Equals(receivedAreaUpdate.AreaId));
                                    if (foundArea != null)
                                        foundArea.IsActive = true;
                                }
                            }
                                break;

                            // Game Ready, show the path
                            case GameEvent.Type.GameReady:
                            {
                                var receivedAreaUpdate = receivedEvent.Data as List<GameEventTuple>;
                                if (receivedAreaUpdate == null)
                                {
                                    // game not ready, show as message
                                    this.ShowInformationOverlay("game not ready");
                                }
                                else
                                {
                                    this.ShowInformationOverlay("game not ready");

                                    this.GameArea.setPath(receivedAreaUpdate.Select(x => new Tuple<int, int>((int)x.Item1, (int)x.Item2)).ToList());

                                    // Show Path step by step
                                    foreach (Area areaPath in this.GameArea.Path)
                                    {
                                        this.GUIGameInstance.AreaList.FirstOrDefault(
                                            area => area.AreaId.Equals(areaPath.AreaId)).IsActive = true;
                                        Wait(2);
                                    }

                                    this.GUIGameInstance.AreaList.Select(i =>
                                    {
                                        i.IsActive = false;
                                        return i;
                                    }).ToList();
                                }
                            }
                            break;
                        }
                    }

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
    }
}