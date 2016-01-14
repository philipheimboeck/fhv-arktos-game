using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e);

    /// <summary>
    /// Event Args containing a player
    /// </summary>
    public class PlayerLeftEventArgs : EventArgs
    {
        public Player Player { get; set; }
    }
}
