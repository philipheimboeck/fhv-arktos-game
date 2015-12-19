using ArctosGameServer.Communication;

namespace ArctosGameServer.Controller
{
    public class RobotController
    {
        private IProtocolLayer protocol;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protocol"></param>
        public RobotController(IProtocolLayer protocol)
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
            PDU pdu = new PDU
            {
                Key = "drive",
                Data = "-50,50"
            };

            if (!this.protocol.send(pdu))
            {
                // error - could not send data
            }
        } 
    }
}