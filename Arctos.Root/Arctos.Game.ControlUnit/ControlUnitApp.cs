using System;
using System.ComponentModel;
using System.Windows;
using Arctos.Game.ControlUnit.Input;
using Arctos.Game.Middleware.Logic.Model.Client;
using Arctos.Game.Middleware.Logic.Model.Model;
using Arctos.Game.Model;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using ArctosGameServer.Controller;

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

            if (_gamepadController.IsConnected()) PlayerStatus = "Connected";
        }

        /// <summary>
        /// Try to esablish a connection to the robot
        /// </summary>
        /// <param name="comPort"></param>
        private void ConnectRobot(string comPort)
        {
            try
            {
                IProtocolLayer<object, object> protocol = new PresentationLayer(
                    new SessionLayer(
                        new TransportLayer(comPort)
                        )
                    );
                _robotController = new RobotController(protocol);
                RobotStatus = "Connected to Port " + comPort;
            }
            catch (Exception ex)
            {
                RobotStatus = ex.Message;
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
                    case "ConnectRobot":
                    {
                        this.ConnectRobot(RobotCOMPort);
                    }
                        break;

                    case "ConnectGame":
                    {
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
                            this.GameStatus = isAvailable ? "Connected" : "Username already taken";
                        }
                    }
                        break;

                    case "Start":
                    {
                        if (!gameStarted)
                        {
                            gameStarted = true;
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
                _gamepadController.Update();

                if (_movementDirty)
                {
                    var left = (int) _gamepadController.GetValue(GamepadController.Wheels.WheelLeft);
                    var right = (int) _gamepadController.GetValue(GamepadController.Wheels.WheelRight);

                    // Drive
                    _robotController.Drive(left, right);
                    _movementDirty = false;
                }

                // Read
                if (_client != null && _client.Connected)
                {
                    try
                    {
                        var rfid = _robotController.ReadRFID();
                        if (!string.IsNullOrEmpty(rfid))
                            _client.Send(new GameEvent(GameEvent.Type.AreaUpdate, rfid));
                    }
                    catch (Exception ex)
                    {
                        _client = null;
                    }
                }

                // Exit Game Loop
                if (worker.CancellationPending)
                {
                    doWorkEventArgs.Cancel = true;
                    gameStarted = false;
                    return;
                }
            }
        }

        #region Properties

        private const string GAME_STATE_START = "Start Game";
        private const string GAME_STATE_STOP = "Stop Game";

        private readonly BackgroundWorker worker = new BackgroundWorker();

        private GamepadController _gamepadController;
        private RobotController _robotController;
        private GameTcpClient _client;
        private bool _movementDirty = false;
        private bool gameStarted = false;

        private string _playerStatus = "Disconnected";

        public string PlayerStatus
        {
            get { return _playerStatus; }
            set
            {
                _playerStatus = value;
                OnPropertyChanged();
            }
        }

        private string _gameStatus = "Disconnected";

        public string GameStatus
        {
            get { return _gameStatus; }
            set
            {
                _gameStatus = value;
                OnPropertyChanged();
            }
        }

        private string _robotStatus = "Disconnected";

        public string RobotStatus
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
                PlayerStatus = "Connected";
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.ControllerDisconnected))
            {
                PlayerStatus = "Disconnected";
            }
        }

        #endregion
    }
}