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

        private List<IObserver<GamepadController.GamepadControllerEvent>> _observers = new List<IObserver<GamepadControllerEvent>>();

        private XBoxControllerWatcher _watcher;
        private XBoxController _controller;
        private Dictionary<Key, double> _oldValues = new Dictionary<Key, double>();

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

        private void Watcher_ControllerConnected(XBoxController controller)
        {
            // Check if controller is not connected
            if(this._controller == null)
            {
                this._controller = controller;

                // Notify about connection
                Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.CONTROLLER_CONNECTED));
            }
        }

        private void Watcher_ControllerDisconnected(XBoxController controller)
        {
            // Check if the current controller is disconnected
            if(this._controller.Equals(controller))
            {
                this._controller = null;

                // Notify about disconnection
                Notify(new GamepadControllerEvent(GamepadControllerEvent.EventType.CONTROLLER_DISCONNECTED));
            }
        }

        public void Update()
        {
            if(_controller != null && _controller.IsConnected)
            {
                // Read values
                double leftTrigger = _controller.TriggerLeftPosition;
                double rightTrigger = _controller.TriggerRightPosition;

                // If the values changed, send them

                if(!_oldValues[Key.TRIGGER_LEFT].Equals(leftTrigger))
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

        public IDisposable Subscribe(IObserver<GamepadControllerEvent> observer)
        {
            // Add observer
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }

            return new Unsubscriber(_observers, observer);
        }

        private void Notify(GamepadControllerEvent eventObject) {
            foreach(IObserver<GamepadControllerEvent> observer in _observers) {
                observer.OnNext(eventObject);
            }
        }

        public class Unsubscriber : IDisposable
        {
            private List<IObserver<GamepadControllerEvent>> _observers;
            private IObserver<GamepadControllerEvent> _observer;

            public Unsubscriber(List<IObserver<GamepadControllerEvent>> observers, IObserver<GamepadControllerEvent> observer)
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
            
            public EventType Type { get; set; }

            public Key? PressedKey { get; set; }

            public double? Value { get; set; }

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
        }
    }
}
