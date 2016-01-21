using System;
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
            PlayerLost,
            PlayerFinish,
            PlayerKicked,
            GuiRequest,
            GuiJoined,
            GuiLeft,
            AreaUpdate,
            GameReady,
            GameStart,
            GameFinish,
            ConnectionLost
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

        [XmlElement("gameArea", Type = typeof (GameArea))]
        [XmlElement("area", Type = typeof (Area))]
        [XmlElement("string", Type = typeof (string))]
        [XmlElement("bool", Type = typeof (bool))]
        [XmlElement("boolStringTuple", Type = typeof (GameEventTuple<bool, string>))]
        [XmlElement("path", Type = typeof (Path))]
        [XmlElement("double", Type = typeof (double))]
        public object Data { get; set; }
    }
}