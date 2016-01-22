using System;

namespace Arctos.Controller.Events
{
    public delegate void PlayerFinishEventHandler(object sender, PlayerFinishEvent e);

    public class PlayerFinishEvent : EventArgs
    {
        public double Duration { get; set; }

        public PlayerFinishEvent(double duration)
        {
            this.Duration = duration;
        }
    }
}