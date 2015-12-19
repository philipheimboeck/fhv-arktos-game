﻿using System;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using ArctosGameServer.Controller;
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
            IProtocolLayer protocol = new PresentationLayer(
                                               new SessionLayer(
                                                   new TransportLayer("COM13")
                                               )
                                           );

            RobotController robotController = new RobotController(protocol);
            robotController.Drive(10, -10);
        }
    }
}
