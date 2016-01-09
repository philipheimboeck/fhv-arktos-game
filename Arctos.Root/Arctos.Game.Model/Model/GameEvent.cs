using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Arctos.Game.Model;

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
            AreaUpdate,
            GameReady,
            GameStart
        }

        public GameEvent()
        {
        }

        public GameEvent(Type type, object data)
        {
            EventType = type;
            Data = data;
        }

        public Type EventType { get; set; }

        [XmlElement("gameArea", Type = typeof(GameArea))]
        [XmlElement("area", Type = typeof(Area))]
        [XmlElement("string", Type = typeof(string))]
        [XmlElement("bool", Type = typeof(bool))]
        [XmlElement("tupleBoolString", Type = typeof(Tuple<bool, string>))]
        [XmlElement("path", Type = typeof(List<Tuple<int, int>>))]
        public object Data { get; set; }
    }
}