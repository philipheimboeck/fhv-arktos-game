using ArctosGameServer.Controller;
using System;
using System.Windows;
using Arctos.Game.ControlUnit.Input;
using Arctos.Game.ControlUnit.View;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using Arctos.Game.Middleware.Logic.Model.Client;

namespace Arctos.Game
{
    public class ControlUnitApp : PropertyChangedBase, IObserver<GamepadController.GamepadControllerEvent>
    {
        #region Properties

        private GamepadController _gamepadController;
        private RobotController _robotController;
        private GameTcpClient _client;
        private bool _movementDirty = false;

        private string _playerStatus = "Disconnected";
        public string PlayerStatus
        {
            get { return _playerStatus; }
            set { _playerStatus = value; OnPropertyChanged(); }
        }
        private string _gameStatus = "Disconnected";
        public string GameStatus
        {
            get { return _gameStatus; }
            set { _gameStatus = value; OnPropertyChanged(); }
        }
        private string _robotStatus = "Disconnected";
        public string RobotStatus
        {
            get { return _robotStatus; }
            set { _robotStatus = value; OnPropertyChanged(); }
        }
        private string _robotRobotCOMPort = "COM33";
        public string RobotCOMPort
        {
            get { return _robotRobotCOMPort; }
            set { _robotRobotCOMPort = value; OnPropertyChanged(); }
        }
        private string _gameIP = "172.22.26.41";
        public string GameIP
        {
            get { return _gameIP; }
            set { _gameIP = value; OnPropertyChanged(); }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comPort"></param>
        public ControlUnitApp(string comPort)
        {
            _gamepadController = new GamepadController();
            _gamepadController.Subscribe(this);

            //this.ConnectRobot(comPort);

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
        /// Start the Control Unit
        /// </summary>
        private void Start()
        {
            // Game Loop
            while(true)
            {
                _gamepadController.Update();

                if(_movementDirty)
                {
                    int left = (int)_gamepadController.GetValue(GamepadController.Key.TRIGGER_LEFT);
                    int right = (int)_gamepadController.GetValue(GamepadController.Key.TRIGGER_RIGHT);

                    // Drive
                    _robotController.Drive(left, right);
                    _movementDirty = false;
                }

                // Read
                if (_client != null && _client.Connected)
                {
                    try
                    {
                        _client.Send(new GameEvent(GameEvent.Type.AREA_UPDATE, _robotController.ReadRFID()));
                    }
                    catch (Exception ex)
                    {
                        _client = null;
                    }
                }
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
                            this.GameStatus = "Connected";
                        }
                        break;

                    case "Start":
                        {
                            this.Start();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
            if(value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.INPUT_CHANGE))
            {
                // Driving values changed, therefore mark as dirty
                _movementDirty = true;
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.CONTROLLER_CONNECTED))
            {
                PlayerStatus = "Connected";
            }

            if (value.Type.Equals(GamepadController.GamepadControllerEvent.EventType.CONTROLLER_DISCONNECTED))
            {
                PlayerStatus = "Disconnected";
            }
        }

        #endregion
    }
}
