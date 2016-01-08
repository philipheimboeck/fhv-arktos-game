using System;
using ArctosGameServer.Communication;

namespace ArctosGameServer.Controller
{
    public class RobotController
    {
        private IProtocolLayer<object, object> protocol;

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

        public string ReadRFID()
        {
            var receivedPDU = this.protocol.receive();
            return (receivedPDU.data == null) ? "" : receivedPDU.data.ToString();
        }
    }
}