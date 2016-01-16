using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ArctosGameServer.Communication.ServerProtocol
{
    public class SlipTransportLayer : ProtocolLayer
    {
        private const byte SLIP_END = (byte) 0xC0;
        private const byte SLIP_ESC = (byte) 0xDB;
        private const byte SLIP_ESC_END = (byte) 0xDC;
        private const byte SLIP_ESC_ESC = (byte) 0xDD;

        private ITcpCommunicator _client;

        public SlipTransportLayer(ITcpCommunicator client) : base(null)
        {
            _client = client;
        }

        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            if (pduInput == null)
            {
                return null;
            }

            var pduOutput = new PDU<object>();

            // Get the string from the above layers
            var data = pduInput.data as string;
            if (data == null)
            {
                return null;
            }

            var byteData = GetBytes(data);
            for (var i = 0; i < byteData.Count; i++)
            {
                var b = byteData[i];

                // Replace all END characters with ESC_END
                if (b == SLIP_END)
                {
                    byteData[i] = SLIP_ESC;
                    byteData.Insert(i + 1, SLIP_ESC_END);
                }

                // Replace all ESC characters with ESC_ESC
                if (b == SLIP_ESC)
                {
                    byteData[i] = SLIP_ESC;
                    byteData.Insert(i + 1, SLIP_ESC_ESC);
                }
            }
            // Add the END character to the end
            byteData.Add(SLIP_END);

            var array = byteData.ToArray();
            pduOutput.data = array;

            return pduOutput;
        }

        protected override PDU<object> decomposePdu(PDU<object> pduInput)
        {
            if (pduInput == null)
            {
                return null;
            }

            var pduOutput = new PDU<object>();

            // Get the string from the bottom layers
            var data = pduInput.data as List<byte>;
            if (data == null)
            {
                return null;
            }

            // Remove the END character from the end
            data.RemoveAt(data.Count - 1);

            for (var i = 0; i < data.Count; i++)
            {
                var b = data[i];

                // Replace all two bytes sequences ESC ESC_ESC with the ESC character
                if (b == SLIP_ESC_ESC && i > 0 && data[i - 1] == SLIP_ESC)
                {
                    data[i] = SLIP_ESC;
                    data.RemoveAt(i - 1);
                }

                // Replace all two bytes sequences ESC ESC_END with the END character
                if (b == SLIP_ESC_END && i > 0 && data[i - 1] == SLIP_ESC)
                {
                    data[i] = SLIP_END;
                    data.RemoveAt(i - 1);
                }
            }

            pduOutput.data = GetString(data.ToArray());

            return pduOutput;
        }

        private static List<byte> GetBytes(string str)
        {
            return new List<byte>(Encoding.Default.GetBytes(str));
        }

        private static string GetString(byte[] bytes)
        {
            return Encoding.Default.GetString(bytes);
        }

        public override PDU<object> receive()
        {
            var receivedData = new List<byte>();

            while (true)
            {
                var data = _client.Read();
                if (data != null)
                {
                    receivedData.Add((byte) data);

                    // Completed reading?
                    if ((byte) data == SLIP_END)
                    {
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            var pdu = new PDU<object>() {data = receivedData};
            return decomposePdu(pdu);
        }

        public override bool send(PDU<object> pdu)
        {
            var pduOut = this.composePdu(pdu);

            var data = pduOut.data as byte[];
            return _client.Write(data);
        }
    }
}