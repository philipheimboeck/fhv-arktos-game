using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using Arctos.Game.ControlUnit.Controller;
using Arctos.Game.ControlUnit.Controller.Events;
using Arctos.Game.ControlUnit.Input;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using RandomNameGenerator;

namespace Arctos.Game.ControlUnit.ViewModel
{
    /// <summary>
    /// ControlUnit ViewModel
    /// </summary>
    public class ControlUnitViewModel : PropertyChangedBase, IObserver<GamepadController.GamepadControllerEvent>
    {
        #region Properties

        private DateTime LastReceivedHeartbeat;

        private bool IsWaitingForRobot = true;

        private Timer HeartbeatTimer;

        private bool IsDriveAllowed { get; set; }

        private const string GAME_CONNECT = "Connect to Game";
        private const string GAME_DISCONNECT = "Disconnect Game";
        private const string ROBOT_WAIT = "Wait for Robot";
        private const string ROBOT_CONNECTED = "Connected to Robot";
        private const string DISCONNECTED = "Disconnected";
        private const string CONNECTED = "Connected";

        private readonly BackgroundWorker ControlUnitLoopWorker = new BackgroundWorker();
        private readonly BackgroundWorker ReceiveEventsLoopWorker = new BackgroundWorker();

        private readonly GamepadController GamepadController;
        private readonly RobotController RobotController;
        private GameTcpClient GameClient;
        private bool _movementDirty;

        private string _log;
        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
                OnPropertyChanged();
            }
        }

        private string _buttonRobotStatus = ROBOT_WAIT;
        public string ButtonRobotStatus
        {
            get { return _buttonRobotStatus; }
            set
            {
                _buttonRobotStatus = value;
                OnPropertyChanged();
            }
        }

        private string _playerStatusText = DISCONNECTED;
        public string PlayerStatusText
        {
            get { return _playerStatusText; }
            set
            {
                _playerStatusText = value;
                OnPropertyChanged();
            }
        }
        private bool _playerStatus;
        public bool PlayerStatus
        {
            get { return _playerStatus; }
            set
            {
                _playerStatus = value;
                OnPropertyChanged();
            }
        }

        private string _gameStatusText = DISCONNECTED;
        public string GameStatusText
        {
            get { return _gameStatusText; }
            set
            {
                _gameStatusText = value;
                OnPropertyChanged();
            }
        }

        private bool _gameStatus;
        public bool GameStatus
        {
            get { return _gameStatus; }
            set
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        private string _playerName = NameGenerator.GenerateFirstName(Gender.Female).ToLower();
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                _playerName = value;
                OnPropertyChanged();
            }
        }

        private string _robotStatusText = DISCONNECTED;
        public string RobotStatusText { get { return _robotStatusText; } set { _robotStatusText = value; OnPropertyChanged(); } }

        private bool _robotStatus;
        public bool RobotStatus
        {
            get { return _robotStatus; }
            set
            {
                _robotStatus = value;
                OnPropertyChanged();
            }
        }

        private string _robotRobotCOMPort = "COM33";
        public string RobotCOMPort
        {
            get { return _robotRobotCOMPort; }
            set
            {
                _robotRobotCOMPort = value;
                OnPropertyChanged();
            }
        }

        private string _gameIP = "172.22.25.119";
        public string GameIP
        {
            get { return _gameIP; }
            set
            {
                _gameIP = value;
                OnPropertyChanged();
            }
        }

        private string _buttonGameStatus = GAME_CONNECT;
        public string ButtonGameStatus
        {
            get { return _buttonGameStatus; }
            set
            {
                _buttonGameStatus = value;
                OnPropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public ControlUnitViewModel() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comPort"></param>
        public ControlUnitViewModel(string comPort)
        {
            try
            {
                // Init Gamepad Controller
                GamepadController = new GamepadController();
                GamepadController.Subscribe(this);

                if (GamepadController.IsConnected())
                {
                    PlayerStatus = true;
                    PlayerStatusText = CONNECTED;
                }

                // Init Robot controller
                RobotController = new RobotController(comPort);
                RobotController.HeartbeatEvent += RobotControllerOnHeartbeatEvent;
                RobotController.RfidEvent += RobotControllerOnRfidEvent;

                // Init ControlUnit Loop worker
                ControlUnitLoopWorker.DoWork += RunControlUnitLoop;
                ControlUnitLoopWorker.WorkerReportsProgress = true;
                ControlUnitLoopWorker.WorkerSupportsCancellation = true;

                // Init Receive Events Loop worker
                ReceiveEventsLoopWorker.DoWork += ReceiveEventsWaiterOnWork;
                ControlUnitLoopWorker.WorkerReportsProgress = true;
                ControlUnitLoopWorker.WorkerSupportsCancellation = true;
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
            }
        }

        /// <summary>
        /// Connect to Game Server
        /// </summary>
        private void ConnectToGame()
        {
            if (this.GameStatus) return;

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += delegate
            {
                try
                {
                    if (GameStatus == false)
                    {
                        LogWrite(LogLevel.Info, "Connect to Server on " + this.GameIP);
                        GameClient = new GameTcpClient(this.GameIP);
                        GameClient.ReceivedDataEvent += GameClientOnReceivedDataEvent;

                        if (GameClient.Connected)
                        {
                            this.SendEvent(GameEvent.Type.PlayerRequest, this.PlayerName);
                            LogWrite(LogLevel.Info, "Ask for Game with Playername = " + this.PlayerName);
                            ReceiveEventsLoopWorker.RunWorkerAsync();
                            //ButtonGameStatus = GAME_DISCONNECT;
                        }
                    }
                    else
                    {
                        //this.ClosedGameserverConnection();
                        ReceiveEventsLoopWorker.CancelAsync();
                    }
                }
                catch (Exception ex)
                {
                    this.ClosedGameserverConnection();
                    LogWrite(LogLevel.Error, ex.Message);
                }
            };
            bgw.RunWorkerAsync();
        }

        /// <summary>
        /// Reset Gamestatus and cleint after connection failure
        /// </summary>
        private void ClosedGameserverConnection()
        {
            ButtonGameStatus = GAME_CONNECT;
            GameStatusText = DISCONNECTED;
            GameStatus = false;
            GameClient = null;

            this.ControlUnitLoopWorker.CancelAsync();
        }

        /// <summary>
        /// Try to esablish a connection to the robot
        /// </summary>
        /// <param name="comPort"></param>
        private void WaitForRobot(string comPort)
        {
            try
            {
                this.ButtonRobotStatus = ROBOT_WAIT;
                this.RobotController.ConnectSerial(this.RobotCOMPort);
            }
            catch (Exception ex)
            {
                this.RobotStatus = false;
                this.ButtonRobotStatus = DISCONNECTED;
                RobotStatusText = ex.Message;
                LogWrite(LogLevel.Error, ex.Message);
            }

            BackgroundWorker bgw = new BackgroundWorker();
            bgw.DoWork += delegate
            {
                try
                {
                    this.RobotStatus = false;
                    this.ControlUnitLoopWorker.CancelAsync();
                    do
                    {
                        RobotController.ReadBluetoothData();
                        // Wait until robot has connected
                    } while (this.IsWaitingForRobot);

                    RobotStatusText = "Connected to Port " + comPort;
                    LogWrite(LogLevel.Info, RobotStatusText);

                    IsDriveAllowed = true;
                    if (this.ControlUnitLoopWorker.IsBusy)
                        this.ControlUnitLoopWorker.CancelAsync();

                    this.ControlUnitLoopWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    this.RobotStatus = false;
                    this.ButtonRobotStatus = DISCONNECTED;
                    RobotStatusText = ex.Message;
                    LogWrite(LogLevel.Error, ex.Message);
                }
            };
            bgw.RunWorkerAsync();
        }

        /// <summary>
        /// Execute Commands from ViewModel
        /// </summary>
        /// <param name="parameter"></param>
        public override void Execute(object parameter)
        {
            try
            {
                switch (parameter.ToString())
                {
                    case "WaitForRobot":
                    {
                        RobotStatusText = ROBOT_WAIT;
                        this.WaitForRobot(RobotCOMPort);
                    }
                        break;

                    case "ConnectGame":
                    {
                        ConnectToGame();
                    }
                        break;
                    case "Discover":
                        Discover();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
            }
        }

        /// <summary>
        /// Received Data from Game Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameClientOnReceivedDataEvent(object sender, ReceivedEventArgs args)
        {
            try
            {
                GameEvent gameEvent = args.Data as GameEvent;
                if (gameEvent == null) return;

                switch (gameEvent.EventType)
                {
                    // Game Ready
                    case GameEvent.Type.GameReady:
                    {
                        RobotController.Drive(0, 0);
                        IsDriveAllowed = false;
                        LogWrite(LogLevel.Events, "Received GameReady");
                    }
                        break;

                    // Game Ready
                    case GameEvent.Type.PlayerKicked:
                        {
                            RobotController.Drive(0, 0);
                            IsDriveAllowed = false;
                            this.ClosedGameserverConnection();
                            LogWrite(LogLevel.Events, "Kicked out from Game by GameServer");
                        }
                        break;

                    // Game Start
                    case GameEvent.Type.GameStart:
                    {
                        IsDriveAllowed = true;
                        LogWrite(LogLevel.Events, "Received GameStart");
                    }
                        break;

                    // Player Joined
                    case GameEvent.Type.PlayerJoined:
                    {
                        var isAvailable = (GameEventTuple<bool, string>) gameEvent.Data;
                        this.GameStatusText = isAvailable.Item1 ? CONNECTED : isAvailable.Item2;
                        this.GameStatus = isAvailable.Item1;
                        this.ButtonGameStatus = GAME_CONNECT;
                        LogWrite(LogLevel.Events, "Received PlayerJoined - " + GameStatusText);
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
            }
        }

        /// <summary>
        /// Wait for Incoming Events
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private void ReceiveEventsWaiterOnWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            try
            {
                while (true)
                {
                    if (ReceiveEventsLoopWorker.CancellationPending || GameClient == null)
                    {
                        doWorkEventArgs.Cancel = true;
                        return;
                    }

                    GameClient.Receive();
                }
            }
            catch (Exception ex)
            {
                this.ClosedGameserverConnection();
                LogWrite(LogLevel.Error, ex.Message);
            }
        }

        private void Discover()
        {
            var task = new Task(DiscoverTask);
            task.Start();
        }

        private void DiscoverTask()
        {
            var client = new DiscoveryServiceClient();
            var ip = client.Discover();
            if (ip != null)
            {
                GameIP = ip;
            }
            else
            {
                LogWrite(LogLevel.Info, "Could not find Service");
            }
        }

        #region Game Events

        /// <summary>
        /// Send player left if we lost connection to the robot
        /// </summary>
        private void SendPlayerLeft()
        {
            this.LogWrite(LogLevel.Info, "Lost connection to Robot, say goodbye to GameServer");
            this.SendEvent(GameEvent.Type.PlayerLeft, null);
        }

        /// <summary>
        /// Got connection to robot back (when there was already a connection joined)
        /// </summary>
        private void SendPlayerRejoined()
        {
            //this.SendEvent(GameEvent.Type.PlayerRejoined, null);
        }

        /// <summary>
        /// Send event to game Helper Utility
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        private void SendEvent(GameEvent.Type type, object data)
        {
            try
            {
                if (GameClient != null && GameClient.Connected)
                {
                    GameClient.Send(new GameEvent(type, data));
                }
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
            }
        }

        #endregion

        #region Robot Events

        /// <summary>
        /// Start the Control Unit and Game Worker process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="doWorkEventArgs"></param>
        private void RunControlUnitLoop(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            // Game Loop
            while (true)
            {
                // Gamepad updates
                GamepadController.Update();
                if (IsDriveAllowed && _movementDirty)
                {
                    var left = (int) GamepadController.GetValue(GamepadController.Wheels.WheelLeft);
                    var right = (int) GamepadController.GetValue(GamepadController.Wheels.WheelRight);
                    
                    // Drive
                    RobotController.Drive(left, right);
                    _movementDirty = false;
                }

                // Read Bluetooth Data
                RobotController.ReadBluetoothData();

                // Exit Game Loop
                if (this.ControlUnitLoopWorker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Send Area Update on RFID read
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receivedDataEventArgs"></param>
        private void RobotControllerOnRfidEvent(object sender, ReceivedDataEventArgs receivedDataEventArgs)
        {
            try
            {
                string rfidTag = receivedDataEventArgs.Data as string;
                if (rfidTag != null)
                {
                    this.SendEvent(GameEvent.Type.AreaUpdate, rfidTag);
                }
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
                GameClient = null;
            }
        }

        /// <summary>
        /// Keep track of the robot heartbeat Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="receivedDataEventArgs"></param>
        private void RobotControllerOnHeartbeatEvent(object sender, ReceivedDataEventArgs receivedDataEventArgs)
        {
            if (this.IsWaitingForRobot) {
                this.ButtonRobotStatus = ROBOT_CONNECTED;
                RobotStatus = true;
                this.HeartbeatTimer = new Timer(1000);
                this.HeartbeatTimer.Elapsed += CheckHeartbeat;
                this.HeartbeatTimer.Start();
            }

            this.LastReceivedHeartbeat = DateTime.Now;
            this.IsWaitingForRobot = false;
        }

        /// <summary>
        /// Check robot's heartbeat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsed"></param>
        private void CheckHeartbeat(object sender, EventArgs elapsed)
        {
            var heartbeatDifference = Math.Abs((DateTime.Now - this.LastReceivedHeartbeat).TotalSeconds);
            if (heartbeatDifference > 3)
            {
                if (this.RobotStatus)
                {
                    this.RobotStatusText = "Did not receive heartbeat within " + heartbeatDifference + " seconds.";
                    this.RobotStatus = false;
                    LogWrite(LogLevel.Warning, this.RobotStatusText);

                    this.SendPlayerLeft();
                }

                LogWrite(LogLevel.Warning, this.RobotStatusText);
            }
            else
            {
                this.RobotStatusText = "Connected to Robot";
                if (this.RobotStatus) return;

                LogWrite(LogLevel.Info, this.RobotStatusText);
                this.SendPlayerRejoined();
                this.RobotStatus = true;
            }
        }

        #endregion

        #region Logging

        /// <summary>
        /// Write a log message to debug window
        /// </summary>
        /// <param name="level"></param>
        /// <param name="Message"></param>
        private void LogWrite(LogLevel level, string Message)
        {
            Log = Log + level + " [" + DateTime.Now.ToLongTimeString() + "] " + Message + "\r\n";
        }

        /// <summary>
        /// Log Levels
        /// </summary>
        enum LogLevel
        {
            Events,
            Error,
            Warning,
            Info
        }

        #endregion

        #region Gamepad Events

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
            LogWrite(LogLevel.Error, "GamepadController Error: " + error.Message);
        }

        public void OnNext(GamepadController.GamepadControllerEvent value)
        {
            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.InputChange))
            {
                // Driving values changed, therefore mark as dirty
                _movementDirty = true;
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.ControllerConnected))
            {
                PlayerStatus = true;
                PlayerStatusText = CONNECTED;
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.ControllerDisconnected))
            {
                PlayerStatus = false;
                PlayerStatusText = DISCONNECTED;
            }
        }

        #endregion
    }
}