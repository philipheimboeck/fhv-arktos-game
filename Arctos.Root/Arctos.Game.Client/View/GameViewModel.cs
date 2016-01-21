using System;
using System.ComponentModel;
using System.Linq;
using Arctos.Controller;
using Arctos.Controller.Events;
using Arctos.Game.GUIClient;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using Arctos.View.Utilities;

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

        private BackgroundWorker ReinitGameStateWorker { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="game"></param>
        public GameViewModel(GameTcpClient client, Game.Middleware.Logic.Model.Model.Game game)
        {
            try
            {
                _controller = new ViewController(client, game.GameArea);
                _controller.AreaUpdateEvent += OnAreaUpdateEvent;
                _controller.PlayerLeftEvent += OnPlayerLeftEvent;
                _controller.GameReadyEvent += OnGameReadyEvent;
                _controller.GameStartEvent += OnGameStartEvent;
                _controller.GameFinishEvent += OnGameFinishEvent;
                _controller.PlayerFinishEvent += OnPlayerFinishEvent;
                _controller.PlayerKickedEvent += ControllerOnPlayerKickedEvent;
                _controller.PlayerLostEvent += ControllerOnPlayerLostEvent;
                _controller.ErrorEvent += ControllerOnErrorEvent;
                
                this.GUIGameInstance = new GuiGameArea(game.GameArea)
                {
                    AreaWidth = 800,
                    AreaHeight = 600
                };

                ReinitGameStateWorker = new BackgroundWorker { WorkerSupportsCancellation = true };
                ReinitGameStateWorker.DoWork += ReinitGameStateWorkerOnDoWork;
                ReinitGameStateWorker.RunWorkerAsync(game);
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay(ex.Message);
                ViewHelper.Wait(4);
                this.CloseTrigger = true;
            }
        }

        /// <summary>
        /// Initialize GameView Model
        /// </summary>
        private void ReinitGameStateWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            Game.Middleware.Logic.Model.Model.Game game = e.Argument as Game.Middleware.Logic.Model.Model.Game;
            if (game != null)
            {
                if (game.State == GameState.Ready)
                {
                    // show game ready info, each game
                    this.ShowEachStepOnStartup(game.Path);
                }
                else if (game.State == GameState.Started)
                {

                }
                else if (game.State == GameState.Waiting)
                {
                    // ignore and wait
                }
            }

            ReinitGameStateWorker.CancelAsync();
        }

        #region Events

        /// <summary>
        /// Received Event from ViewController
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="errorEventArgs"></param>
        private void ControllerOnErrorEvent(object sender, ErrorEventArgs errorEventArgs)
        {
            this._controller.GameClient = null;

            this.ShowInformationOverlay(errorEventArgs.Message);

            ViewHelper.Wait(4);

            this.CloseTrigger = true;
        }

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

            ViewHelper.Wait(4);

            this.CloseTrigger = true;
        }

        /// <summary>
        /// GameReady Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gameReadyEventArgs"></param>
        private void OnGameReadyEvent(object sender, GameReadyEventArgs gameReadyEventArgs)
        {
            try
            {
                var receivedAreaUpdate = gameReadyEventArgs.Path;
                if (receivedAreaUpdate == null)
                {
                    // game not ready, show as message
                    this.ShowInformationOverlay("Game is not ready. Please Wait.");
                }
                else
                {
                    this.ShowEachStepOnStartup(receivedAreaUpdate);
                }
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay(ex.Message);
            }
        }

        /// <summary>
        /// Show each Area step by step on Startup
        /// </summary>
        /// <param name="receivedAreaUpdate"></param>
        private void ShowEachStepOnStartup(Path receivedAreaUpdate)
        {
            this.ShowInformationOverlay("READY ....");

            _controller.GameArea.setPath(receivedAreaUpdate.Waypoints.Select(x => new Tuple<int, int>(x.Item1, x.Item2)).ToList());

            // Show Path step by step
            foreach (Area areaPath in _controller.GameArea.Path)
            {
                var guiArea =
                    this.GUIGameInstance.AreaList.FirstOrDefault(area => area.AreaId.Equals(areaPath.AreaId));
                if (guiArea != null)
                    guiArea.Status = Area.AreaStatus.CorrectlyPassed;

                ViewHelper.Wait(1);
            }
        }

        /// <summary>
        /// PlayerLost Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerLostEventArgs"></param>
        private void ControllerOnPlayerLostEvent(object sender, PlayerLostEventArgs playerLostEventArgs)
        {
            this.ShowInformationOverlay(playerLostEventArgs.IsLost
                ? "Please wait.. you lost the connection to your Robot."
                : "Welcome back your Robot!");
        }

        /// <summary>
        /// PlayerKicked Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerKickedEventArgs"></param>
        private void ControllerOnPlayerKickedEvent(object sender, PlayerKickedEventArgs playerKickedEventArgs)
        {
            // View is invalid now, close it
            this.ShowInformationOverlay("You got kicked from the Game.. closing now");
            _controller.SendEvent(GameEvent.Type.GuiLeft, null);
            this.CloseTrigger = true;
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
        }

        /// <summary>
        /// AreaUpdate Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="areaUpdateEventArgs"></param>
        private void OnAreaUpdateEvent(object sender, AreaUpdateEventArgs areaUpdateEventArgs)
        {
            try
            {
                var receivedAreaUpdate = areaUpdateEventArgs.Area;
                if (receivedAreaUpdate == null) return;

                GuiArea foundArea = this.GUIGameInstance.AreaList.FirstOrDefault(x => x.AreaId.Equals(receivedAreaUpdate.AreaId));
                if (foundArea == null) return;

                if (receivedAreaUpdate.Status == Area.AreaStatus.CorrectlyPassed)
                {
                    // When correctly passing a field, reset all wrongly passed field
                    foreach (var area in GUIGameInstance.AreaList)
                    {
                        area.Status = Area.AreaStatus.None;
                    }

                    foreach (var area in GUIGameInstance.Path)
                    {
                        GuiArea foundPathArea = this.GUIGameInstance.AreaList.FirstOrDefault(x => x.AreaId.Equals(area.AreaId));
                        if (area.Equals(foundArea))
                        {
                            break;
                        }

                        if (foundPathArea != null)
                            foundPathArea.Status = Area.AreaStatus.CorrectlyPassed;
                    }
                }

                foundArea.Status = receivedAreaUpdate.Status;
            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay(ex.Message);
            }
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

            ViewHelper.Wait(2);

            this.ShowGameInformation = false;
            this.GameInformation = "";
        }

        #endregion
    }
}