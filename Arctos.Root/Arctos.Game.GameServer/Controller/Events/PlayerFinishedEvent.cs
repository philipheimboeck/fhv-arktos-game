using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void PlayerFinishedEventHandler(object sender, PlayerFinishedEventArgs e);

    /// <summary>
    /// Event Args containing a player
    /// </summary>
    public class PlayerFinishedEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }
}