using System.Collections.Generic;
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
        public void Drive(double left, double right)
        {
            // TODO generate proper values and check if they are correct!
            KeyValuePair<string, string> keyValue = new KeyValuePair<string, string>("drive", "-50, 50");
            PDU<object> pduObj = new PDU<object> { data = keyValue };

            if (!this.protocol.send(pduObj))
            {
                // error - could not send data
            }
        }

        public void ReadRFID()
        {
            //PDU pdu = new PDU();
           // this.protocol.receive(pdu);
        }
    }
}