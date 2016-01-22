using System;

namespace Arctos.Controller.Events
{
    public delegate void GameResetEventHandler(object sender, GameResetEventArgs e);

    public class GameResetEventArgs : EventArgs
    {
    }
}