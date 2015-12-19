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
            try
            {
                this.serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                this.serialPort.DataReceived += SerialPortOnDataReceived;
                this.serialPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR - " + ex.Message);
            }
        }

        public override bool receive(PDU pdu)
        {

            return false;
        }

        /// <summary>
        /// OnDataReceived Event of the serial port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="serialDataReceivedEventArgs"></param>
        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            int dataLength = this.serialPort.BytesToRead;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send data via transport layer (serial) to robot
        /// </summary>
        /// <param name="pdu"></param>
        /// <returns></returns>
        public override bool send(PDU pdu)
        {
            bool result = false;

            PDU pduInput = composePdu(pdu);
            if (pduInput == null) return false;

            try
            {
                this.serialPort.Write(pduInput.ComposedData);
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
        protected override PDU composePdu(PDU pduInput)
        {
            return pduInput;
        }

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected override void decomposePdu(PDU pduInput)
        {
            throw new System.NotImplementedException();
        }
    }
}