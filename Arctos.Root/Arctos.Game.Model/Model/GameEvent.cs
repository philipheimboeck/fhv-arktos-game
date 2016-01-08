using System;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    [Serializable]
    public class GameEvent<T>
    {
        

        public GameEvent()
        {
        }

        public GameEvent(GameEventType gameEventType, T data)
        {
            EventGameEventType = gameEventType;
            Data = data;
        }

        public GameEventType EventGameEventType { get; set; }

        public T Data { get; set; }
    }
}