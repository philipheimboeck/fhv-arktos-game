using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerLostEventHandler(object sender, PlayerLostEventArgs e);

    public class PlayerLostEventArgs : EventArgs
    {
        public bool IsLost { get; set; }

        public PlayerLostEventArgs(bool isLost)
        {
            this.IsLost = isLost;
        }
    }
}