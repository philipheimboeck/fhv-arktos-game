using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Arctos.Controller;
using Arctos.Controller.Events;
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

        private ViewController _controller;

        private bool closeTrigger;
        public bool CloseTrigger
        {
            get { return this.closeTrigger; }
            set
            {
                this.closeTrigger = value;
                OnPropertyChanged();
            }
        }

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


        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="area"></param>
        public GameViewModel(GameTcpClient client, GameArea area)
        {
            try
            {
                _controller = new ViewController(client, area);
                _controller.AreaUpdateEvent += OnAreaUpdateEvent;
                _controller.PlayerLeftEvent += OnPlayerLeftEvent;
                _controller.GameReadyEvent += OnGameReadyEvent;
                _controller.GameStartEvent += OnGameStartEvent;
                _controller.GameFinishEvent += OnGameFinishEvent;
                _controller.PlayerFinishEvent += OnPlayerFinishEvent;

                this.GUIGameInstance = new GuiGameArea(area)
                {
                    AreaWidth = 800,
                    AreaHeight = 600
                };

                InitializeGameViewModel();
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay(ex.Message);
            }
        }
    
        /// <summary>
        /// Initialize GameView Model
        /// </summary>
        private void InitializeGameViewModel()
        {
        }

        #region Events

        /// <summary>
        /// PlayerFinish Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerFinishEventArgs"></param>
        private void OnPlayerFinishEvent(object sender, PlayerFinishEventArgs playerFinishEventArgs)
        {
            this.ShowInformationOverlay("You finished this Game in " + playerFinishEventArgs.Duration / 1000 + " seconds.");
        }
        
        /// <summary>
        /// GameFinish Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gameFinishEventArgs"></param>
        private void OnGameFinishEvent(object sender, GameFinishEventArgs gameFinishEventArgs)
        {
            this.ShowInformationOverlay(gameFinishEventArgs.Won
                ? "CONGRATULATION - you won"
                : "SORRY - you lost this game");

            Wait(4);

            this.CloseTrigger = true;
        }

        /// <summary>
        /// GameReady Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gameReadyEventArgs"></param>
        private void OnGameReadyEvent(object sender, GameReadyEventArgs gameReadyEventArgs)
        {
            var receivedAreaUpdate = gameReadyEventArgs.Path;
            if (receivedAreaUpdate == null)
            {
                // game not ready, show as message
                this.ShowInformationOverlay("Game is not ready. Please Wait.");
            }
            else
            {
                this.ShowInformationOverlay("READY ....");

                _controller.GameArea.setPath(
                    receivedAreaUpdate.Waypoints.Select(x => new Tuple<int, int>(x.Item1, x.Item2)).ToList());

                // Show Path step by step
                foreach (Area areaPath in _controller.GameArea.Path)
                {
                    var guiArea = this.GUIGameInstance.AreaList.FirstOrDefault(area => area.AreaId.Equals(areaPath.AreaId));
                    if (guiArea != null)
                        guiArea.Status = Area.AreaStatus.CorrectlyPassed;

                    Wait(1);
                }
            }
        }

        /// <summary>
        /// GameStart Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gameStartEventArgs"></param>
        private void OnGameStartEvent(object sender, GameStartEventArgs gameStartEventArgs)
        {
            this.GUIGameInstance.AreaList.Select(i =>
            {
                i.Status = Area.AreaStatus.None;
                return i;
            }).ToList();
        }

        /// <summary>
        /// PlayerLeft Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerLeftEventArgs"></param>
        private void OnPlayerLeftEvent(object sender, PlayerLeftEventArgs playerLeftEventArgs)
        {
            // View is invalid now, close it
            this.ShowInformationOverlay("Player has left the game.. closing now");
            Wait(3);
            _controller.SendEvent(GameEvent.Type.GuiLeft, null);
            this.CloseTrigger = true;
        }

        /// <summary>
        /// AreaUpdate Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="areaUpdateEventArgs"></param>
        private void OnAreaUpdateEvent(object sender, AreaUpdateEventArgs areaUpdateEventArgs)
        {
            var receivedAreaUpdate = areaUpdateEventArgs.Area;
            if (receivedAreaUpdate == null) return;

            GuiArea foundArea = this.GUIGameInstance.AreaList.FirstOrDefault(x => x.AreaId.Equals(receivedAreaUpdate.AreaId));
            if (foundArea == null) return;

            if (receivedAreaUpdate.Status == Area.AreaStatus.WronglyPassed)
            {
                _controller.WrongPath.Add(new Tuple<GuiArea, Area.AreaStatus>(foundArea, foundArea.Status));
            }
            else
            {
                foreach (var area in _controller.WrongPath)
                {
                    area.Item1.Status = area.Item2;
                }
                _controller.WrongPath.Clear();
            }

            foundArea.Status = receivedAreaUpdate.Status;
        }

        #endregion

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