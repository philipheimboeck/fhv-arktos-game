using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void GuiChangedEventHandler(object sender, GuiChangedEventArgs e);

    public class GuiChangedEventArgs : EventArgs
    {
        public Player Player { get; private set; }

        public GuiChangedEventArgs(Player player)
        {
            Player = player;
        }
    }
}