using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerJoinedEventHandler(object sender, PlayerJoinedEventArgs e);

    public class PlayerJoinedEventArgs : EventArgs
    {
    }
}