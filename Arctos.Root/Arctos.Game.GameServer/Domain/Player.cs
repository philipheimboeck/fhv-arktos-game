using System;
using Arctos.Game.Model;

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
