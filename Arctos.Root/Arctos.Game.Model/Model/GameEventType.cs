using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Game.Middleware.Logic.Model.Model
{
    public enum GameEventType
    {
        PlayerRequest,
        PlayerJoined,
        PlayerLeft,
        GuiRequest,
        GuiJoined,
        AreaUpdate
    }
}
