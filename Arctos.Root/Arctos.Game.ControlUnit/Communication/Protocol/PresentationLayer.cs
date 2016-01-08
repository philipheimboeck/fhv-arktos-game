using System;
using System.Text;

namespace ArctosGameServer.Communication.Protocol
{
    /// <summary>
    /// Presentation Layer
    /// </summary>
    public class PresentationLayer : ProtocolLayer
    {
        private const int ProtocolVersion = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lower"></param>
        public PresentationLayer(IProtocolLayer<object, object> lower)
            : base(lower)
        {
        }

        /// <summary>
        /// Compose PDU from given input
        /// </summary>
        /// <param name="pduInput"></param>
        /// <returns></returns>
        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            PDU<object> pduCommand = null;

            var dataTuple = pduInput.data as Tuple<string, string>;
            if (dataTuple != null)
            {
                var version = string.Format("{0:D2}", ProtocolVersion);
                var dataLength = string.Format("{0:D3}", dataTuple.Item2.Length);
                var keyLength = string.Format("{0:D3}", dataTuple.Item1.Length);

                var composedData = new StringBuilder();
                composedData.Append(version)
                    .Append(keyLength)
                    .Append(dataLength)
                    .Append(dataTuple.Item1)
                    .Append(dataTuple.Item2);

                pduCommand = new PDU<object>
                {
                    data = composedData.ToString()
                };
            }

            return pduCommand;
        }

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected override PDU<object> decomposePdu(PDU<object> pduInputData)
        {
            Tuple<string, string> pduDecomposed = null;
            if (pduInputData != null && pduInputData.data != null)
            {
                var pduInput = new string((char[]) pduInputData.data).TrimEnd('\0');
                var version = pduInput.Substring(0, 2);
                var keyLength = int.Parse(pduInput.Substring(2, 3));
                var dataLength = int.Parse(pduInput.Substring(5, 3));

                var key = pduInput.Substring(8, keyLength);
                var commandValue = pduInput.Substring(8 + keyLength, dataLength);

                pduDecomposed = new Tuple<string, string>(key, commandValue);
            }

            return new PDU<object> {data = pduDecomposed};
        }
    }
}