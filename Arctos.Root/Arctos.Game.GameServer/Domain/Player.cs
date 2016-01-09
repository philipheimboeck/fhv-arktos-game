using System;
using Arctos.Game.Model;

namespace ArctosGameServer.Domain
{
    public class Player
    {
        public string Name { get; set; }

        public Guid ControlUnitId { get; set; }

        public Guid GuiId { get; set; }

        public GameArea Map { get; set; }

        public bool PlayerReady { get; set; }
    }
}