using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Arctos.Game.Middleware.Logic.Model.Model;

namespace ArctosGameServer.Communication.ServerProtocol
{
    public class PresentationLayer : ProtocolLayer
    {
        public PresentationLayer(IProtocolLayer<object, object> lower) : base(lower)
        {
        }

        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            PDU<object> pduOutput = null;

            var data = pduInput.data as GameEvent;
            if (data != null)
            {
                var composedData = new StringBuilder();

                // Serialize the input
                var serializer = new XmlSerializer(typeof (GameEvent));
                try
                {
                    using (var writer = new StringWriter(composedData))
                    {
                        serializer.Serialize(writer, data);
                    }

                    pduOutput = new PDU<object> {data = composedData.ToString()};
                }
                catch (Exception ex)
                {
                    // Todo Error handling
                }
            }

            return pduOutput;
        }

        protected override PDU<object> decomposePdu(PDU<object> pduInput)
        {
            PDU<object> pduOutput = null;
            
            var data = pduInput.data as string;
            if (data != null)
            {
                var composedData = new StringBuilder();

                // Serialize the input
                var serializer = new XmlSerializer(typeof(GameEvent));
                try
                {
                    using (var reader = new StringReader(data))
                    {
                        pduOutput = new PDU<object> {data = serializer.Deserialize(reader)};
                    }
                }
                catch (Exception ex)
                {
                    // Todo Error handling
                }
            }

            return pduOutput;
        }
    }
}