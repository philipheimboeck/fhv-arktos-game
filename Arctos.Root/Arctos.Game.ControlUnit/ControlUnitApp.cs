using System;
using System.ComponentModel;
using System.Drawing;
using System.Timers;
using System.Windows;
using Arctos.Game.ControlUnit.Input;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using ArctosGameServer.Controller;
using ArctosGameServer.Controller.Events;

namespace Arctos.Game
{
    public class ControlUnitApp : PropertyChangedBase, IObserver<GamepadController.GamepadControllerEvent>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comPort"></param>
        public ControlUnitApp(string comPort)
        {
            _gamepadController = new GamepadController();
            _gamepadController.Subscribe(this);

            if (_gamepadController.IsConnected())
            {
                PlayerStatus = true;
                PlayerStatusText = "Connected";
            }
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Try to esablish a connection to the robot
        /// </summary>
        /// <param name="comPort"></param>
        private void WaitForRobot(string comPort)
        {
            try
            {
                IProtocolLayer<object, object> protocol = new PresentationLayer(
                    new SessionLayer(
                        new TransportLayer(comPort)
                        )
                    );

                _robotController = new RobotController(protocol);
                _robotController.HeartbeatEvent += RobotControllerOnHeartbeatEvent;
                _robotController.RfidEvent += RobotControllerOnRfidEvent;

                BackgroundWorker bgw = new BackgroundWorker();
                bgw.DoWork += delegate(object sender, DoWorkEventArgs args)
                {
                    RobotStatus = false;
                    do
                    {
                        _robotController.ReadBluetoothData();
                        // Wait until robot has connected
                    } while (IsWaitingForRobot);

                    RobotStatusText = "Connected to Port " + comPort;
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
                        RobotStatusText = "Waiting for robot connection";
                        this.WaitForRobot(RobotCOMPort);
                    }
                        break;

                    case "ConnectGame":
                    {
                        if (IsWaitingForRobot) return;

                        _client = new GameTcpClient(this.GameIP);

                        if (_client.Connected)
                        {
                            _client.Send(new GameEvent(GameEvent.Type.PlayerRequest, "ninos"));

                            GameEvent gameEvent;
                            do
                            {
                                gameEvent = _client.Receive();
                            } while (gameEvent != null && gameEvent.EventType != GameEvent.Type.PlayerJoined);
                            if (gameEvent == null) return;

                            var isAvailable = (bool) gameEvent.Data;
                            this.GameStatusText = isAvailable ? "Connected" : "Username already taken";
                            this.GameStatus = isAvailable;
                        }
                    }
                        break;

                    case "Start":
                    {
                        if (IsWaitingForRobot) return;

                        if (!GameStatus)
                        {
                            GameStatus = true;
                            worker.DoWork += StartGame;
                            worker.WorkerReportsProgress = true;
                            worker.WorkerSupportsCancellation = true;
                            worker.RunWorkerAsync();
                            CurrentGameStatus = GAME_STATE_STOP;
                        }
                        else
                        {
                            CurrentGameStatus = GAME_STATE_START;
                            worker.CancelAsync();
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
        private void StartGame(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            // Game Loop
            while (true)
            {
                // Gamepad updates
                _gamepadController.Update();
                if (_movementDirty)
                {
                    var left = (int) _gamepadController.GetValue(GamepadController.Wheels.WheelLeft);
                    var right = (int) _gamepadController.GetValue(GamepadController.Wheels.WheelRight);

                    // Drive
                    _robotController.Drive(left, right);
                    _movementDirty = false;
                }

                // Read Bluetooth Data
                _robotController.ReadBluetoothData();

                // Exit Game Loop
                if (worker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    GameStatus = false;
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
            if (IsWaitingForRobot) { 
                RobotStatus = true;
                this.HeartbeatTimer = new Timer(1000);
                this.HeartbeatTimer.Elapsed += CheckHeartbeat;
                this.HeartbeatTimer.Start();
            }

            this.LastReceivedHeartbeat = DateTime.Now;
            IsWaitingForRobot = false;
        }

        /// <summary>
        /// Check robot's heartbeat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="elapsed"></param>
        private void CheckHeartbeat(object sender, EventArgs elapsed)
        {
            var heartbeatDifference = System.Math.Abs((DateTime.Now - this.LastReceivedHeartbeat).TotalSeconds);
            this.RobotStatusText = "Did not receive heartbeat within " + heartbeatDifference + " seconds.";

            if (heartbeatDifference > 3)
            {
                //this.RobotStatus = false;
                this.RobotStatusText = "Did not receive heartbeat within " + heartbeatDifference + " seconds.";
            }
        }


        #region Properties

        private DateTime LastReceivedHeartbeat;

        private bool IsWaitingForRobot = true;

        private Timer HeartbeatTimer;

        private const string GAME_STATE_START = "Start Game";
        private const string GAME_STATE_STOP = "Stop Game";

        private readonly BackgroundWorker worker = new BackgroundWorker();

        private GamepadController _gamepadController;
        private RobotController _robotController;
        private GameTcpClient _client;
        private bool _movementDirty = false;

        private string _playerStatusText = "Disconnected";
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

        private string _gameStatusText = "Disconnected";
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

        private string _robotStatusText = "Disconnected";
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

        private string _currentGameStatus = GAME_STATE_START;
        public string CurrentGameStatus
        {
            get { return _currentGameStatus; }
            set
            {
                _currentGameStatus = value;
                OnPropertyChanged();
            }
        }

        #endregion

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
                PlayerStatusText = "Connected";
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.ControllerDisconnected))
            {
                PlayerStatus = false;
                PlayerStatusText = "Disconnected";
            }
        }

        #endregion
    }
}