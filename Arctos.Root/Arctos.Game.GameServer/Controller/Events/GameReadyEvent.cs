using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void GameReadyEventHandler(object sender, GameReadeEventArgs e);

    /// <summary>
    /// Event Args containing a player
    /// </summary>
    public class GameReadeEventArgs : EventArgs
    {
        public bool Ready { get; set; }
    }
}
