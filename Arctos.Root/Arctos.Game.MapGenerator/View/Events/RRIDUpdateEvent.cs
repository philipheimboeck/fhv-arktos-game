using System;

namespace Arctos.Game.MapGenerator.View.Events
{
    public delegate void RFIDUpdateEventHandler(object sender, RFIDUpdateEventArgs e);

    public class RFIDUpdateEventArgs : EventArgs
    {
        public String RFID { get; set; }

        public RFIDUpdateEventArgs(String rfid)
        {
            RFID = rfid;
        }
    }
}
