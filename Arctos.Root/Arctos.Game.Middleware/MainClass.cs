using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using ArctosGameServer.Controller;
using ArctosGameServer.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArctosGameServer
{
    class MainClass : IObserver<GamepadController.GamepadControllerEvent>
    {
        private GamepadController _gamepadController;
        private RobotController _robotController;
        private bool _movementDirty = false;

        public MainClass(String comPort)
        {
            _gamepadController = new GamepadController();
            _gamepadController.Subscribe(this);

            IProtocolLayer<object, object> protocol = new PresentationLayer(
                                                            new SessionLayer(
                                                                new TransportLayer(comPort)
                                                            )
                                                        );

            _robotController = new RobotController(protocol);
        }

        public void Start()
        {
            // Game Loop
            while(true)
            {
                _gamepadController.Update();

                if(_movementDirty)
                {
                    double left = _gamepadController.GetValue(GamepadController.Key.TRIGGER_LEFT);
                    double right = _gamepadController.GetValue(GamepadController.Key.TRIGGER_RIGHT);

                    // Drive
                    _robotController.Drive(left, right);
                    _movementDirty = false;
                }
            }
        }

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
        }

        static void Main(string[] args)
        {
            string comPort = "COM1";
            if(args.Length > 0)
            {
                comPort = args[0];
            }

            MainClass process = new MainClass(comPort);
            process.Start();
        }
    }
}
