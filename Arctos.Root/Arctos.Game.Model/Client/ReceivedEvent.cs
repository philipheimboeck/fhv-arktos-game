using System;

namespace Arctos.Game.Middleware.Logic.Model.Client
{
    public delegate void ReceivedEvent(object sender, ReceivedEventArgs e);

    public class ReceivedEventArgs : EventArgs
    {
        public object Data { get; set; }
    }
}