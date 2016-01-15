using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerFinishEventHandler(object sender, PlayerFinishEventArgs e);

    public class PlayerFinishEventArgs : EventArgs
    {
        public double Duration { get; set; }

        public PlayerFinishEventArgs(double duration)
        {
            this.Duration = duration;
        }
    }
}