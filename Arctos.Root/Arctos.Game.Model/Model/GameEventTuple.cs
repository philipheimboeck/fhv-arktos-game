using System;
using System.Xml.Serialization;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    [Serializable]
    public class GameEventTuple
    {
        [XmlElement("string", Type = typeof(string))]
        [XmlElement("bool", Type = typeof(bool))]
        [XmlElement("int", Type = typeof(int))]
        public object Item1 { get; set; }

        [XmlElement("string", Type = typeof(string))]
        [XmlElement("bool", Type = typeof(bool))]
        [XmlElement("int", Type = typeof(int))]
        public object Item2 { get; set; }
    }
}