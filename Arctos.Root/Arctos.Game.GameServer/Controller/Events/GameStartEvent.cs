using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void GameStartEventHandler(object sender, GameStartEventArgs e);

    /// <summary>
    /// Event Args
    /// </summary>
    public class GameStartEventArgs : EventArgs
    {
        public bool Ready { get; set; }
    }
}
