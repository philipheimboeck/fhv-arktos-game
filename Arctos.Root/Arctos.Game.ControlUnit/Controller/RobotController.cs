using System;
using ArctosGameServer.Communication;
using ArctosGameServer.Controller.Events;

namespace ArctosGameServer.Controller
{
    public class RobotController
    {
        private IProtocolLayer<object, object> protocol;

        public event ReadDataEventHandler RfidEvent;
        public event ReadDataEventHandler HeartbeatEvent;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocol"></param>
        public RobotController(IProtocolLayer<object, object> protocol)
        {
            this.protocol = protocol;
        }

        /// <summary>
        /// Move robot usign both motors, left and right
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void Drive(int left, int right)
        {
            // TODO generate proper values and check if they are correct!
            var keyValue = new Tuple<string, string>("drive", left + "," + right);
            var pduObj = new PDU<object> {data = keyValue};

            if (!this.protocol.send(pduObj))
            {
                // error - could not send data
            }
        }

        protected virtual void OnRfidEvent(ReceivedDataEventArgs e)
        {
            if (this.RfidEvent != null) this.RfidEvent(this, e);
        }

        protected virtual void OnHeartbeatEvent(ReceivedDataEventArgs e)
        {
            if (this.HeartbeatEvent != null) this.HeartbeatEvent(this, e);
        }

        /// <summary>
        /// Read data from bluetooth stream
        /// </summary>
        /// <returns></returns>
        public void ReadBluetoothData()
        {
            PDU<object> receivedPDU = this.protocol.receive();
            if (receivedPDU == null || receivedPDU.Key == null) return;

            if (receivedPDU.Key.Equals("live"))
            {
                this.OnHeartbeatEvent(new ReceivedDataEventArgs());
            }

            if (receivedPDU.Key.Equals("rfid"))
            {
                this.OnRfidEvent(new ReceivedDataEventArgs { Data = receivedPDU.data.ToString() });
            }
        }
    }
}