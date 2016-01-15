using System;
using Arctos.Game.Model;

namespace Arctos.Controller.Events
{
    public delegate void AreaUpdateEventHandler(object sender, AreaUpdateEventArgs e);

    public class AreaUpdateEventArgs : EventArgs
    {
        public Area Area { get; set; }

        public AreaUpdateEventArgs(Area area)
        {
            Area = area;
        }
    }
}