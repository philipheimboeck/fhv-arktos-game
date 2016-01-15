using System;
using System.Collections.Generic;
using System.Linq;
using BrandonPotter.XBox;

namespace Arctos.Game.ControlUnit.Input
{
    public class GamepadController : IObservable<GamepadController.GamepadControllerEvent>
    {
        public enum Wheels
        {
            WheelLeft,
            WheelRight
        };

        private XBoxController _controller;

        private List<IObserver<GamepadController.GamepadControllerEvent>> _observers =
            new List<IObserver<GamepadControllerEvent>>();

        private Dictionary<Wheels, double> _movementValues = new Dictionary<Wheels, double>();

        private XBoxControllerWatcher _watcher;

        private const int TURN_SPEED = 25;

        public GamepadController()
        {
            _watcher = new XBoxControllerWatcher();
            _watcher.ControllerConnected += Watcher_ControllerConnected;
            _watcher.ControllerDisconnected += Watcher_ControllerDisconnected;

            // Check for already connected controllers
            _controller = XBoxController.GetConnectedControllers().FirstOrDefault();

            // Initialize movement values
            _movementValues.Add(Wheels.WheelLeft, 0);
            _movementValues.Add(Wheels.WheelRight, 0);
        }

        public IDisposable Subscribe(IObserver<GamepadControllerEvent> observer)
        {
            // Add observer
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber(_observers, observer);
        }

        private void Watcher_ControllerConnected(XBoxController controller)
        {
            // Check if controller is not connected
            if (this._controller == null)
            {
                this._controller = controller;

                // Notify about connection
                Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.ControllerConnected));
            }
        }

        private void Watcher_ControllerDisconnected(XBoxController controller)
        {
            // Check if the current controller is disconnected
            if (this._controller.Equals(controller))
            {
                this._controller = null;

                // Notify about disconnection
                Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.ControllerDisconnected));
            }
        }

        public bool IsConnected()
        {
            return (_controller != null && _controller.IsConnected);
        }

        public void Update()
        {
            if (_controller != null && _controller.IsConnected)
            {
                // Read values
                var leftTrigger = _controller.TriggerLeftPosition;
                var rightTrigger = _controller.TriggerRightPosition;
                var leftButton = _controller.ButtonShoulderLeftPressed;
                var rightButton = _controller.ButtonShoulderRightPressed;

                var notification = false;
                // If any sholder button is pressed suppress the triggers
                if (leftButton || rightButton)
                {
                    if (leftButton && rightButton)
                    {
                        // If both buttons are pressed drive backwards
                        _movementValues[Wheels.WheelLeft] = -TURN_SPEED;
                        _movementValues[Wheels.WheelRight] = -TURN_SPEED;
                    }
                    else if (rightButton)
                    {
                        // If only right button pressed turn right
                        _movementValues[Wheels.WheelLeft] = TURN_SPEED;
                        _movementValues[Wheels.WheelRight] = -TURN_SPEED;
                    }
                    else
                    {
                        // If only left button pressed turn left
                        _movementValues[Wheels.WheelLeft] = -TURN_SPEED;
                        _movementValues[Wheels.WheelRight] = TURN_SPEED;
                    }

                    notification = true;
                }
                else
                {
                    // Send movement by triggers
                    if (!_movementValues[Wheels.WheelRight].Equals(leftTrigger))
                    {
                        _movementValues[Wheels.WheelRight] = leftTrigger;
                    }

                    if (!_movementValues[Wheels.WheelLeft].Equals(rightTrigger))
                    {
                        _movementValues[Wheels.WheelLeft] = rightTrigger;
                    }
                    
                    notification = true;
                }

                if (notification)
                {
                    Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.InputChange));
                }
            }
        }

        public double GetValue(Wheels wheels)
        {
            return _movementValues[wheels];
        }

        private void Notify(GamepadControllerEvent eventObject)
        {
            foreach (var observer in _observers)
            {
                observer.OnNext(eventObject);
            }
        }

        public class Unsubscriber : IDisposable
        {
            private IObserver<GamepadControllerEvent> _observer;
            private List<IObserver<GamepadControllerEvent>> _observers;

            public Unsubscriber(List<IObserver<GamepadControllerEvent>> observers,
                IObserver<GamepadControllerEvent> observer)
            {
                _observers = observers;
                _observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null)
                {
                    _observers.Remove(_observer);
                }
            }
        }

        public class GamepadControllerEvent
        {
            public enum EventType
            {
                ControllerDisconnected,
                ControllerConnected,
                InputChange
            };

            public GamepadControllerEvent(EventType type)
            {
                Type = type;
            }

            public EventType Type { get; set; }

            public Wheels? PressedWheels { get; set; }

            public double? Value { get; set; }
        }
    }
}