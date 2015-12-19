using System;
using System.Collections.Generic;
using System.Linq;
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

            Tuple<string, string> dataTuple = pduInput.data as Tuple<string, string>;
            if (dataTuple != null)
            {
                string version = string.Format("{0:D2}", ProtocolVersion);
                string dataLength = string.Format("{0:D3}", dataTuple.Item2.Length);
                string keyLength = string.Format("{0:D3}", dataTuple.Item1.Length);

                StringBuilder composedData = new StringBuilder();
                composedData.Append(version).Append(keyLength).Append(dataLength).Append(dataTuple.Item1).Append(dataTuple.Item2);

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
            if (pduInputData != null)
            {
                string pduInput = pduInputData.data.ToString();
                string version = pduInput.Take(2).ToString();
                int dataLength = int.Parse(pduInput.Skip(2).Take(3).ToString());
                int keyLength = int.Parse(pduInput.Skip(3).Take(3).ToString());

                string key = pduInput.Skip(8).Take(keyLength).ToString();
                string commandValue = pduInput.Skip(8 + keyLength).Take(dataLength).ToString();

                pduDecomposed = new Tuple<string, string>(key, commandValue);
            }

            return new PDU<object> {data = pduDecomposed};
        }
    }
}