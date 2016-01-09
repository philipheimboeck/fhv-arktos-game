using System;
using System.Windows.Forms;
using ArctosGameServer.Communication;
using ArctosGameServer.Communication.Protocol;
using ArctosGameServer.Controller.Events;

namespace Arctos.Game.ControlUnit.Controller
{
    public class RobotController
    {
        #region Properties

        private IProtocolLayer<object, object> protocol;

        public event ReadDataEventHandler RfidEvent;
        public event ReadDataEventHandler HeartbeatEvent;

        private string ComPort { get; set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="comPort"></param>
        public RobotController(string comPort)
        {
            this.ComPort = comPort;
        }

        /// <summary>
        /// Establish a serial connection to COM-Port
        /// </summary>
        public void ConnectSerial(string comPort)
        {
            if (!string.IsNullOrEmpty(comPort))
                this.ComPort = comPort;

            try
            {
                IProtocolLayer<object, object> protocolLayer = new PresentationLayer(
                    new SessionLayer(
                        new TransportLayer(this.ComPort)
                        )
                    );

                this.protocol = protocolLayer;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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

        private void OnRfidEvent(ReceivedDataEventArgs e)
        {
            if (this.RfidEvent != null) this.RfidEvent(this, e);
        }

        private void OnHeartbeatEvent(ReceivedDataEventArgs e)
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
            if (receivedPDU == null || receivedPDU.data == null) return;

            Tuple<string, string> tupleData = (Tuple<string, string>) receivedPDU.data;

            if (tupleData.Item1.Equals("live"))
            {
                this.OnHeartbeatEvent(new ReceivedDataEventArgs());
            }

            if (tupleData.Item1.Equals("rfid"))
            {
                this.OnRfidEvent(new ReceivedDataEventArgs { Data = tupleData.Item2 });
            }
        }
    }
}