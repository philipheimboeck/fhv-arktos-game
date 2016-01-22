using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
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

        private ViewController Controller { get; set; }

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

        private string _playerTime = "00:00";
        public string PlayerTime
        {
            get { return _playerTime; }
            set
            {
                _playerTime = value;
                OnPropertyChanged();
            }
        }

        private Stopwatch StopWatchTimer;
        private DispatcherTimer GameTimer;

        private BackgroundWorker ReinitGameStateWorker { get; set; }

        #endregion

        /// <summary>
        /// Parameterless Constructor for WPF
        /// </summary>
        public GameViewModel() { }

        /// <summary>
        /// Debug Constructor
        /// </summary>
        /// <param name="game"></param>
        public GameViewModel(Game.Middleware.Logic.Model.Model.Game game, string playerName)
        {
            try
            {
                StartStopwatch(true);

                this.PlayerName = playerName;

                this.GUIGameInstance = new GuiGameArea(game.GameArea)
                {
                    AreaWidth = 800,
                    AreaHeight = 600
                };


            }
            catch (Exception ex)
            {
                this.ShowInformationOverlay(ex.Message);
                ViewHelper.Wait(4);
                this.CloseTrigger = true;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="client"></param>
        /// <param name="game"></param>
        /// <param name="playerName"></param>
        public GameViewModel(GameTcpClient client, Game.Middleware.Logic.Model.Model.Game game, string playerName)
        {
            try
            {
                this.PlayerName = playerName;

                Controller = new ViewController(client, game.GameArea);
                Controller.AreaUpdateEvent += OnAreaUpdateEvent;
                Controller.PlayerLeftEvent += OnPlayerLeftEvent;
                Controller.GameReadyEvent += OnGameReadyEvent;
                Controller.GameStartEvent += OnGameStartEvent;
                Controller.GameResetEvent += ControllerOnGameResetEvent;
                Controller.GameFinishEvent += OnGameFinishEvent;
                Controller.PlayerFinishEvent += OnPlayerFinishEvent;
                Controller.PlayerKickedEvent += ControllerOnPlayerKickedEvent;
                Controller.PlayerLostEvent += ControllerOnPlayerLostEvent;
                Controller.ErrorEvent += ControllerOnErrorEvent;
                Controller.PlayerJoinedEvent += ControllerOnPlayerJoinedEvent;
                
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

        /// <summary>
        /// Start and show the Stopwatch
        /// </summary>
        private void StartStopwatch(bool withReset)
        {
            if (this.GameTimer == null || withReset) 
            { 
                this.GameTimer = new DispatcherTimer();
                this.GameTimer.Tick += delegate
                {
                    TimeSpan t = TimeSpan.FromMilliseconds(this.StopWatchTimer.ElapsedMilliseconds);
                    string answer = string.Format("{0:D2}m : {1:D2}s",
                        t.Minutes,
                        t.Seconds);
                    PlayerTime = answer;
                };
                this.GameTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            }

            if (this.StopWatchTimer == null || withReset) 
                this.StopWatchTimer = new Stopwatch();

            this.StopWatchTimer.Start();
            this.GameTimer.Start();
        }

        /// <summary>
        /// Pause Stopwatch
        /// </summary>
        private void PauseStopwatch()
        {
            if (this.StopWatchTimer != null) this.StopWatchTimer.Stop();
            if(this.GameTimer != null) this.GameTimer.Stop();
        }

        /// <summary>
        /// Pause Stopwatch
        /// </summary>
        private void ResetStopwatch()
        {
            if (this.StopWatchTimer != null) this.StopWatchTimer.Stop();
            if(this.GameTimer != null) this.GameTimer.Stop();
            this.PlayerTime = "00m : 00s";
        }

        #region Events

        /// <summary>
        /// Game Reset Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="gameResetEventArgs"></param>
        private void ControllerOnGameResetEvent(object sender, GameResetEventArgs gameResetEventArgs)
        {
            this.HideInformationOverlay();
            this.ResetStopwatch();
            this.ResetGame();
            this.ShowInformationOverlay("Game is Over! Wait for new Games..");
        }


        /// <summary>
        /// Reset to Empty Game
        /// </summary>
        private void ResetGame()
        {
            if (this.GUIGameInstance == null) return;

            this.GUIGameInstance.Path = new List<GuiArea>();
            foreach (GuiArea area in this.GUIGameInstance.AreaList)
            {
                area.Status = Area.AreaStatus.None;
            }
        }

        /// <summary>
        /// Received Event from ViewController
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="errorEventArgs"></param>
        private void ControllerOnErrorEvent(object sender, ErrorEventArgs errorEventArgs)
        {
            this.Controller.GameClient = null;

            this.ShowInformationOverlay(errorEventArgs.Message);

            ViewHelper.Wait(4);

            this.CloseTrigger = true;
        }

        /// <summary>
        /// PlayerFinish Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerFinishEventArgs"></param>
        private void OnPlayerFinishEvent(object sender, PlayerFinishEvent playerFinishEventArgs)
        {
            this.PauseStopwatch();
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
                : "SORRY - you lost this game", true);
            
            //this.CloseTrigger = true;
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

            //Controller.GameArea.setPath(receivedAreaUpdate.Waypoints.Select(x => new Tuple<int, int>(x.Item1, x.Item2)).ToList());
            this.GUIGameInstance.SetPath(receivedAreaUpdate.Waypoints.Select(x => new Tuple<int, int>(x.Item1, x.Item2)).ToList());

            // Show Path step by step
            foreach (Area areaPath in Controller.GameArea.Path)
            {
                var guiArea =
                    this.GUIGameInstance.AreaList.FirstOrDefault(area => area.AreaId.Equals(areaPath.AreaId));
                if (guiArea != null)
                    guiArea.Status = Area.AreaStatus.CorrectlyPassed;

                ViewHelper.Wait(1);
            }

            this.StartStopwatch(true);
        }

        /// <summary>
        /// Player joined after connection failure
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerJoinedEventArgs"></param>
        private void ControllerOnPlayerJoinedEvent(object sender, PlayerJoinedEventArgs playerJoinedEventArgs)
        {
            this.ShowInformationOverlay("Welcome Back!");
            this.StartStopwatch(false);
        }

        /// <summary>
        /// PlayerLost Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerLostEventArgs"></param>
        private void ControllerOnPlayerLostEvent(object sender, PlayerLostEventArgs playerLostEventArgs)
        {
            this.ShowInformationOverlay("Please wait... lost the connection to your Control Unit.", true);
            this.PauseStopwatch();
        }

        /// <summary>
        /// PlayerLeft Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerLeftEventArgs"></param>
        private void OnPlayerLeftEvent(object sender, PlayerLeftEventArgs playerLeftEventArgs)
        {
            this.ShowInformationOverlay("Please wait... lost the connection to your Robot.", true);
            this.PauseStopwatch();
        }

        /// <summary>
        /// PlayerKicked Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="playerKickedEventArgs"></param>
        private void ControllerOnPlayerKickedEvent(object sender, PlayerKickedEventArgs playerKickedEventArgs)
        {
            // View is invalid now, close it
            this.PauseStopwatch();
            this.ShowInformationOverlay("You got kicked from the Game.. closing now");
            Controller.SendEvent(GameEvent.Type.GuiLeft, null);
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

            this.StartStopwatch(true);
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
        private void ShowInformationOverlay(string message, bool keepMessageVisible)
        {
            if (keepMessageVisible)
            {
                this.GameInformation = message;
                this.ShowGameInformation = true;
            }
            else
            {
                this.ShowInformationOverlay(message);
            }
        }

        /// <summary>
        /// Hides the message overlay
        /// </summary>
        private void HideInformationOverlay()
        {
            this.ShowGameInformation = false;
            this.GameInformation = "";
        }
        
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