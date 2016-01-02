using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ArctosGameServer.Communication
{
    [Serializable]
    public class GameEvent
    {
        public enum Type
        {
            MAP,
            AREA_UPDATE,
            GAME_STATUS_UPDATE
        }

        public Type EventType { get; }

        public ISerializable Data { get; }

        public GameEvent(Type type, ISerializable data)
        {
            EventType = type;
            Data = data;
        }
    }
}
