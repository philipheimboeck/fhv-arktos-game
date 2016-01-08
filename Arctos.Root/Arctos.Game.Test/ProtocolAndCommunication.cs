using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Arctos.Game.Middleware.Test
{
    [TestClass]
    public class ProtocolAndCommunication
    {
        /// <summary>
        /// Call the protocol layer stack
        /// This test is only used for manual debug testing!
        /// </summary>
        [TestMethod]
        public void CallAllProtocolLayers()
        {
            IProtocolLayer<object, object> protocol = new PresentationLayer(
                new SessionLayer(
                    new TransportLayer("COM33")
                    )
                );


            protocol.receive();

            /*RobotController robotController = new RobotController(protocol);
            while (true) { 
                robotController.Drive(50, -50);

                robotController.Drive(20, -50);

                robotController.Drive(20, 20);

                robotController.Drive(100, 100);

                robotController.Drive(0, 0);
            }*/
        }
    }
}