using System;

namespace ArctosGameServer.Controller.Events
{
    public delegate void ReadDataEventHandler(object sender, ReceivedDataEventArgs e);
    public class ReceivedDataEventArgs : EventArgs
    {
        public object Data { get; set; }
    }
}