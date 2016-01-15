using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace ArctosGameServer.Communication.ServerProtocol
{
    public class TransportLayer : ProtocolLayer
    {
        private ITcpCommunicator _client;

        public TransportLayer(ITcpCommunicator client) : base(null)
        {
            _client = client;
        }

        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            return pduInput;
        }

        protected override PDU<object> decomposePdu(PDU<object> pduInput)
        {
            return pduInput;
        }

        public override PDU<object> receive()
        {
            var message = new StringBuilder();

            int sleepRetries = 0;
            while (true)
            {
                var data = _client.Read();
                if (data != null)
                {
                    message.Append(data);
                }
                else if (message.Length > 0)
                {
                    // Completed reading
                    if (message.ToString().EndsWith("</GameEvent>") || sleepRetries > 5)
                    {
                        break;
                    }

                    // TODO: improve XML protocol in order to avoid incomplete data packets!
                    Thread.Sleep(1000);
                    sleepRetries++;
                }
            }

            var xml = message.ToString();
            if (!xml.EndsWith("</GameEvent>"))
            {
                throw new Exception("Did receive a invalid packet!", new Exception(xml));
            }

            return new PDU<object>() {data = xml};
        }

        public override bool send(PDU<object> pdu)
        {
            var data = pdu.data as string;
            return _client.Write(data);
        }
    }
}