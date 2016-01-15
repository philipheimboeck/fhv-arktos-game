using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerFinishEventHandler(object sender, PlayerFinishEventArgs e);

    public class PlayerFinishEventArgs : EventArgs
    {
        public TimeSpan Duration { get; set; }

        public PlayerFinishEventArgs(TimeSpan duration)
        {
            this.Duration = duration;
        }
    }
}