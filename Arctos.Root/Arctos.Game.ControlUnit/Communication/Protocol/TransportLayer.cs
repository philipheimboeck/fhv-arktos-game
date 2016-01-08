using System;
using System.IO.Ports;
using System.Windows;

namespace ArctosGameServer.Communication.Protocol
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

            //this.serialPort.ReadTimeout = 500;
            //this.serialPort.WriteTimeout = 500;

            this.serialPort.Open();
        }

        public override PDU<object> receive()
        {
            var pduReceived = new PDU<object>();
            if (this.serialPort.BytesToRead > 0)
            {
                var dataReceived = new char[128];
                this.serialPort.Read(dataReceived, 0, 128);
                pduReceived.data = dataReceived;
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