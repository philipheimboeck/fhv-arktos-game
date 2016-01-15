using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerLeftEventHandler(object sender, PlayerLeftEventArgs e);

    public class PlayerLeftEventArgs : EventArgs
    {
    }
}