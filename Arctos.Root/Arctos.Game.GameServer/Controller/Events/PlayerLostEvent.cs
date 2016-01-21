using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void PlayerLostEventHandler(object sender, PlayerLostEventArgs e);

    /// <summary>
    /// Event Args containing a player
    /// </summary>
    public class PlayerLostEventArgs : EventArgs
    {
        public Player Player { get; set; }

        public bool Lost { get; set; }
    }
}
