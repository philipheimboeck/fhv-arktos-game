using System;
using System.Collections.Generic;
using System.Linq;
using BrandonPotter.XBox;

namespace Arctos.Game.ControlUnit.Input
{
    public class GamepadController : IObservable<GamepadController.GamepadControllerEvent>
    {
        public enum Key
        {
            TRIGGER_LEFT,
            TRIGGER_RIGHT
        };

        private XBoxController _controller;

        private List<IObserver<GamepadController.GamepadControllerEvent>> _observers =
            new List<IObserver<GamepadControllerEvent>>();

        private Dictionary<Key, double> _oldValues = new Dictionary<Key, double>();

        private XBoxControllerWatcher _watcher;

        public GamepadController()
        {
            _watcher = new XBoxControllerWatcher();
            _watcher.ControllerConnected += Watcher_ControllerConnected;
            _watcher.ControllerDisconnected += Watcher_ControllerDisconnected;

            // Check for already connected controllers
            _controller = XBoxController.GetConnectedControllers().FirstOrDefault();

            // Initialize old values
            _oldValues.Add(Key.TRIGGER_LEFT, 0);
            _oldValues.Add(Key.TRIGGER_RIGHT, 0);
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
                Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.CONTROLLER_CONNECTED));
            }
        }

        private void Watcher_ControllerDisconnected(XBoxController controller)
        {
            // Check if the current controller is disconnected
            if (this._controller.Equals(controller))
            {
                this._controller = null;

                // Notify about disconnection
                Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.CONTROLLER_DISCONNECTED));
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

                // If the values changed, send them

                if (!_oldValues[Key.TRIGGER_LEFT].Equals(leftTrigger))
                {
                    _oldValues[Key.TRIGGER_LEFT] = leftTrigger;

                    var notification = new GamepadControllerEvent(Key.TRIGGER_LEFT, leftTrigger);
                    Notify(notification);
                }

                if (!_oldValues[Key.TRIGGER_RIGHT].Equals(rightTrigger))
                {
                    _oldValues[Key.TRIGGER_RIGHT] = rightTrigger;

                    var notification = new GamepadControllerEvent(Key.TRIGGER_RIGHT, rightTrigger);
                    Notify(notification);
                }
            }
        }

        public double GetValue(Key key)
        {
            return _oldValues[key];
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
                CONTROLLER_DISCONNECTED,
                CONTROLLER_CONNECTED,
                INPUT_CHANGE
            };

            public GamepadControllerEvent(EventType type)
            {
                Type = type;
            }

            public GamepadControllerEvent(Key key, double value)
            {
                Type = EventType.INPUT_CHANGE;
                PressedKey = key;
                Value = value;
            }

            public EventType Type { get; set; }

            public Key? PressedKey { get; set; }

            public double? Value { get; set; }
        }
    }
}