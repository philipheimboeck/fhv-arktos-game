using System;

namespace Arctos.Controller.Events
{
    public delegate void GameFinishEventHandler(object sender, GameFinishEventArgs e);

    public class GameFinishEventArgs : EventArgs
    {
        public bool Won { get; set; }

        public GameFinishEventArgs(bool won)
        {
            this.Won = won;
        }
    }
}