using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e);

    /// <summary>
    /// Event Args containing a player
    /// </summary>
    public class PlayerJoinedEventArgs : EventArgs
    {
        public Player Player { get; }

        public PlayerJoinedEventArgs(Player player)
        {
            Player = player;
        }
    }
}
