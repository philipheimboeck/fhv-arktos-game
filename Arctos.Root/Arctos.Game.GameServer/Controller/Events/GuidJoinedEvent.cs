using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void GuiJoinedEventHandler(object sender, GuidJoinedEventArgs e);

    public class GuidJoinedEventArgs : EventArgs
    {
        public Player Player { get; private set; }

        public GuidJoinedEventArgs(Player player)
        {
            Player = player;
        }
    }
}