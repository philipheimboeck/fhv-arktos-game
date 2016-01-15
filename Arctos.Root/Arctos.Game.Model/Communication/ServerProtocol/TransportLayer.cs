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

            while (true)
            {
                var data = _client.Read();
                if (data != null)
                {
                    message.Append(data);

                    // Completed reading
                    if (message.ToString().EndsWith("</GameEvent>"))
                    {
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            var xml = message.ToString();
            return new PDU<object>() {data = xml};
        }

        public override bool send(PDU<object> pdu)
        {
            var data = pdu.data as string;
            return _client.Write(data);
        }
    }
}