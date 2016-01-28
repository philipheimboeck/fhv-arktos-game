using System;
using System.ComponentModel;
using System.Timers;
using Arctos.Game.Model;
using Arctos.Game.ControlUnit.Input;
using Arctos.Game.ControlUnit.Controller;
using Arctos.Game.ControlUnit.Controller.Events;
using Arctos.Game.MapGenerator.View.Events;

namespace Arctos.Game.MapGenerator.View
{
    /// <summary>
    /// ControlUnit ViewModel
    /// </summary>
    public class RobotViewModel : PropertyChangedBase, IObserver<GamepadController.GamepadControllerEvent>
    {
        #region Properties

        private DateTime LastReceivedHeartbeat;

        private bool IsWaitingForRobot = true;

        private Timer HeartbeatTimer;
        
        private const string ROBOT_WAIT = "Wait for Robot";
        private const string ROBOT_CONNECTED = "Connected to Robot";
        private const string DISCONNECTED = "Disconnected";
        private const string CONNECTED = "Connected";

        private readonly BackgroundWorker ControlUnitLoopWorker = new BackgroundWorker();
        private readonly BackgroundWorker ReceiveEventsLoopWorker = new BackgroundWorker();

        private readonly GamepadController GamepadController;
        private readonly RobotController RobotController;
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

        private bool _gamepadStatus;
        public bool GamepadStatus
        {
            get
            {
                return _gamepadStatus;
            }

            set
            {
                _gamepadStatus = value;
                OnPropertyChanged();
            }
        }


        private GameView _gameView;
        private GameView GameView
        {
            get
            {
                return _gameView;
            }

            set
            {
                _gameView = value;
            }
        }
        
        private int _gameViewColumns = 3;
        public int GameViewColumns
        {
            get
            {
                return _gameViewColumns;
            }

            set
            {
                _gameViewColumns = value;
            }
        }

        private int _gameViewRows = 3;
        public int GameViewRows
        {
            get
            {
                return _gameViewRows;
            }

            set
            {
                _gameViewRows = value;
            }
        }
        
        public event RFIDUpdateEventHandler RFIDUpdateEvent;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public RobotViewModel() { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comPort"></param>
        public RobotViewModel(string comPort)
        {
            try
            {
                // Init Gamepad Controller
                GamepadController = new GamepadController();
                GamepadController.Subscribe(this);

                if (GamepadController.IsConnected())
                {
                    GamepadStatus = true;
                }

                // Init Robot controller
                RobotController = new RobotController(comPort);
                RobotController.HeartbeatEvent += RobotControllerOnHeartbeatEvent;
                RobotController.RfidEvent += RobotControllerOnRfidEvent;

                // Init ControlUnit Loop worker
                ControlUnitLoopWorker.DoWork += RunControlUnitLoop;
                ControlUnitLoopWorker.WorkerReportsProgress = true;
                ControlUnitLoopWorker.WorkerSupportsCancellation = true;
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
            }
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

        #region Commands

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
                    case "GenerateView":
                        {
                            if (GameViewColumns > 0 && GameViewRows > 0)
                            {
                                if (GameView != null)
                                {
                                    GameView.Hide();
                                }
                                GameView = new GameView(this, GameViewColumns, GameViewRows);
                                GameView.Show();
                            }
                        }
                        break;
                    default:
                        break;
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
                if (this.ControlUnitLoopWorker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Send RFID Update on RFID read
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
                    if (RFIDUpdateEvent != null) RFIDUpdateEvent(this, new RFIDUpdateEventArgs(rfidTag));
                }
            }
            catch (Exception ex)
            {
                LogWrite(LogLevel.Error, ex.Message);
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
                    this.RobotStatus = false;
                    LogWrite(LogLevel.Warning, this.RobotStatusText);
                }

                this.RobotStatusText = "Did not receive heartbeat within " + heartbeatDifference + " seconds.";
            }
            else
            {
                this.RobotStatusText = "Connected to Robot";
                if (this.RobotStatus) return;

                LogWrite(LogLevel.Info, this.RobotStatusText);
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
                GamepadStatus = true;
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.ControllerDisconnected))
            {
                GamepadStatus = false;
            }
        }

        #endregion
    }
}