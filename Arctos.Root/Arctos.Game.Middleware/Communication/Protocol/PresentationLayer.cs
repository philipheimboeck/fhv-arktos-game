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
        public PresentationLayer(IProtocolLayer lower)
            : base(lower)
        {
        }

        /// <summary>
        /// Compose PDU from given input
        /// </summary>
        /// <param name="pduInput"></param>
        /// <returns></returns>
        protected override PDU composePdu(PDU pduInput)
        {
            pduInput.Version = ProtocolVersion;
            pduInput.DataLength = pduInput.Data.Length;
            pduInput.KeyLength = pduInput.Key.Length;

            string version = string.Format("{0:D2}", pduInput.Version);
            string dataLength = string.Format("{0:D3}", pduInput.DataLength);
            string keyLength = string.Format("{0:D3}", pduInput.KeyLength);

            StringBuilder composedData = new StringBuilder();
            composedData.Append(version).Append(keyLength).Append(dataLength).Append(pduInput.Key).Append(pduInput.Data);
            pduInput.ComposedData = composedData.ToString();

            return pduInput;
        }

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected override void decomposePdu(PDU pduInput)
        {
        }
    }
}