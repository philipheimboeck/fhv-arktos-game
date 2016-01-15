using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void GameFinishedEventHandler(object sender, GameFinishEventArgs e);

    /// <summary>
    /// Event Args
    /// </summary>
    public class GameFinishEventArgs : EventArgs
    {
        public bool Finished { get; set; }
    }
}
