using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arctos.Game.Client;
using Arctos.Game.GUIClient;

namespace ArctosGameServer.Domain
{
    class Player
    {
        public string Name { get; set; }
        
        public Guid ControlUnitId { get; set; }

        public Guid GuiId { get; set; }

        public GameArea Map { get; set; } 
    }
}
