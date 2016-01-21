using System;
using System.IO.Ports;
using System.Windows;
using ArctosGameServer.Communication;

namespace Arctos.Game.ControlUnit.Communication.Protocol
{
    /// <summary>
    /// Transport Layer
    /// </summary>
    public class TransportLayer : ProtocolLayer
    {
        private readonly SerialPort serialPort;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="portName"></param>
        public TransportLayer(string portName) : base(null)
        {
            this.serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            this.serialPort.Open();
        }

        public override PDU<object> receive()
        {
            var pduReceived = new PDU<object>();
            if (this.serialPort.BytesToRead > 0)
            {
                char[] frameHeader = new char[8];
                this.serialPort.Read(frameHeader, 0, 8);

                var pduInput = new string(frameHeader);

                var keyLength = int.Parse(pduInput.Substring(2, 3));
                var dataLength = int.Parse(pduInput.Substring(5, 3));

                char[] frameBuffer = new char[120];
                this.serialPort.Read(frameBuffer, 0, keyLength + dataLength);

                char[] frame = new char[128];
                for (int i = 0; i < 8; i++)
                {
                    frame[i] = frameHeader[i];
                }
                for (int i = 0; i < (keyLength + dataLength); i++)
                {
                    frame[i + 8] = frameBuffer[i];
                }

                pduReceived.data = frame;
            }

            return pduReceived;
        }

        /// <summary>
        /// Send data via transport layer (serial) to robot
        /// </summary>
        /// <param name="pdu"></param>
        /// <returns></returns>
        public override bool send(PDU<object> pdu)
        {
            var result = false;

            var pduInput = composePdu(pdu);
            if (pduInput == null || pduInput.data == null) return false;

            try
            {
                this.serialPort.Write(pduInput.data.ToString());
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR - " + ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Compose PDU from given input
        /// </summary>
        /// <param name="pduInput"></param>
        /// <returns></returns>
        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            return pduInput;
        }

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected override PDU<object> decomposePdu(PDU<object> pduInput)
        {
            return pduInput;
        }
    }
}