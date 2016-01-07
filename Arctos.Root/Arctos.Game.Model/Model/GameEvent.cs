using System;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    [Serializable]
    public class GameEvent
    {
        public enum Type
        {
            PlayerRequest,
            PlayerJoined,
            PlayerLeft,
            GuiRequest,
            GuiJoined,
            AreaUpdate
        }

        public Type EventType { get; set; }

        public object Data { get; set; }

        public GameEvent()
        {
        }

        public GameEvent(Type type, object data)
        {
            EventType = type;
            Data = data;
        }
    }
}
