using System;

namespace Arctos.Game.ControlUnit.Controller.Events
{
    public delegate void ReadDataEventHandler(object sender, ReceivedDataEventArgs e);
    public class ReceivedDataEventArgs : EventArgs
    {
        public object Data { get; set; }
    }
}