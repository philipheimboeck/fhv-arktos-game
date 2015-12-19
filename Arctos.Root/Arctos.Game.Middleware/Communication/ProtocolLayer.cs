namespace ArctosGameServer.Communication
{
    public abstract class ProtocolLayer : IProtocolLayer
    {
        private readonly IProtocolLayer lowerLayer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lower"></param>
        protected ProtocolLayer(IProtocolLayer lower)
        {
            this.lowerLayer = lower;
        }

        /// <summary>
        /// Send data to the next lower layer
        /// </summary>
        /// <param name="pdu"></param>
        /// <returns></returns>
        public virtual bool send(PDU pdu)
        {
            bool result = false;
            PDU pduOut = this.composePdu(pdu);

            if (this.lowerLayer != null)
            {
                result = this.lowerLayer.send(pduOut);
            }

            return result;
        }

        /// <summary>
        /// Receive data and decompose PDU
        /// </summary>
        /// <param name="pdu"></param>
        /// <returns></returns>
        public virtual bool receive(PDU pdu)
        {
            bool result = this.lowerLayer.receive(pdu);
            if (result)
                this.decomposePdu(pdu);

            return result;
        }

        /// <summary>
        //  Compose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        /// <returns></returns>
        protected abstract PDU composePdu(PDU pduInput);

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected abstract void decomposePdu(PDU pduInput);
    }
}