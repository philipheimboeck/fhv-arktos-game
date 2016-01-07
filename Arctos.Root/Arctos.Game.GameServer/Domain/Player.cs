using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArctosGameServer.Domain
{
    class Player
    {
        public string Name { get; set; }
        
        public Guid ControlUnitId { get; set; }

        public Guid GuiId { get; set; }
    }
}
