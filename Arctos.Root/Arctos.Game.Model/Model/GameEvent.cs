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

        public Type EventType { get; set; }

        public Object Data { get; set; }

        public GameEvent()
        {
        }

        public GameEvent(Type type, Object data)
        {
            EventType = type;
            Data = data;
        }
    }
}
