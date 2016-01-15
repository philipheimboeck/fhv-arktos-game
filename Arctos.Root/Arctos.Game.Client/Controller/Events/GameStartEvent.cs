using System;

namespace Arctos.Controller.Events
{
    public delegate void GameStartEventHandler(object sender, GameStartEventArgs e);

    public class GameStartEventArgs : EventArgs
    {
    }
}