using ArctosGameServer.Communication;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ArctosGameServer.Controller
{
    /// <summary>
    /// The GameController
    /// </summary>
    public class GameController : IObserver<GameEvent>
    {
        private ConcurrentQueue<GameEvent> _receivedEvents = new ConcurrentQueue<GameEvent>();
        public bool ShutdownRequested { get; set; } = false;

        /// <summary>
        /// Generates a new map
        /// </summary>
        public void GenerateGame()
        {
            // GameConfiguration
        }

        public void loop()
        {
            while (!ShutdownRequested)
            {
                // Process all received events
                GameEvent e = null;
                while (_receivedEvents.TryDequeue(out e)) 
                {
                    switch(e.EventType)
                    {
                        case GameEvent.Type.AREA_UPDATE:
                            {
                                String area = (String)e.Data;
                                UpdateArea(area);
                            }
                            break;
                        default:
                            break;
                    }
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

        public void OnNext(GameEvent value)
        {
            // Receive GameEvent
            _receivedEvents.Enqueue(value);
        }

        private void UpdateArea(string areaID)
        {
            Console.WriteLine("Update area: " + areaID);
        }
    }
}