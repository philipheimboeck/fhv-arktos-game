using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using Arctos.Game.ControlUnit.Controller;
using Arctos.Game.ControlUnit.Input;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using ArctosGameServer.Controller.Events;
using RandomNameGenerator;

namespace Arctos.Game
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

        private const string GAME_CONNECT = "Connect to Game";
        private const string GAME_DISCONNECT = "Disconnect Game";
        private const string ROBOT_WAIT = "Wait for Robot";
        private const string ROBOT_CONNECTED = "Connected to Robot";
        private const string DISCONNECTED = "Disconnected";
        private const string CONNECTED = "Connected";

        private readonly BackgroundWorker controlUnitLoopWorker = new BackgroundWorker();
        private readonly GamepadController GamepadController;
        private readonly RobotController RobotController;
        private GameTcpClient _client;
        private bool _movementDirty = false;

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

        private string _gameIP = "172.22.25.74";
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
            controlUnitLoopWorker.DoWork += RunControlUnitLoop;
            controlUnitLoopWorker.WorkerReportsProgress = true;
            controlUnitLoopWorker.WorkerSupportsCancellation = true;
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

                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += delegate
                {
                    this.RobotStatus = false;
                    this.controlUnitLoopWorker.CancelAsync();
                    do
                    {
                        RobotController.ReadBluetoothData();
                        // Wait until robot has connected
                    } while (this.IsWaitingForRobot);

                    RobotStatusText = "Connected to Port " + comPort;
                    this.controlUnitLoopWorker.RunWorkerAsync();
                };
                bgw.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                RobotStatusText = ex.Message;
            }
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
                        if (GameStatus == false)
                        {
                            _client = new GameTcpClient(this.GameIP);

                            if (_client.Connected)
                            {
                                _client.Send(new GameEvent(GameEvent.Type.PlayerRequest, this.PlayerName));

                                GameEvent gameEvent;
                                do
                                {
                                    gameEvent = _client.Receive();
                                } while (gameEvent != null && gameEvent.EventType != GameEvent.Type.PlayerJoined);
                                if (gameEvent == null) return;

                                var isAvailable = (Tuple<bool, string>) gameEvent.Data;
                                this.GameStatusText = isAvailable.Item1 ? CONNECTED : isAvailable.Item2;
                                this.GameStatus = isAvailable.Item1;
                                this.ButtonGameStatus = GAME_CONNECT;
                            }
                        }
                        else
                        {
                            ButtonGameStatus = GAME_DISCONNECT;
                            GameStatus = false;
                            _client = null;
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
                if (_movementDirty)
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
                if (this.controlUnitLoopWorker.CancellationPending)
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
            if (_client == null || !_client.Connected) return;

            try
            {
                string rfidTag = receivedDataEventArgs.Data as string;
                if(rfidTag != null)
                    _client.Send(new GameEvent(GameEvent.Type.AreaUpdate, rfidTag));
            }
            catch (Exception ex)
            {
                _client = null;
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
            var heartbeatDifference = System.Math.Abs((DateTime.Now - this.LastReceivedHeartbeat).TotalSeconds);
            if (heartbeatDifference > 3)
            {
                this.RobotStatus = false;
                this.RobotStatusText = "Did not receive heartbeat within " + heartbeatDifference + " seconds.";
            }
        }

        #region Events

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
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