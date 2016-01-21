using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerKickedEventHandler(object sender, PlayerKickedEventArgs e);

    public class PlayerKickedEventArgs : EventArgs
    {

    }
}