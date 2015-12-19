namespace ArctosGameServer.Communication.Protocol
{
    /// <summary>
    /// Session Layer
    /// </summary>
    public class SessionLayer : ProtocolLayer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lower"></param>
        public SessionLayer(IProtocolLayer lower)
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