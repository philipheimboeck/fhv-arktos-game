namespace ArctosGameServer.Communication
{
    public abstract class ProtocolLayer : IProtocolLayer<object, object>
    {
        protected IProtocolLayer<object, object> lowerLayer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="lower"></param>
        protected ProtocolLayer(IProtocolLayer<object, object> lower)
        {
            this.lowerLayer = lower;
        }

        /// <summary>
        /// Send data to the next lower layer
        /// </summary>
        /// <param name="pdu"></param>
        /// <returns></returns>
        public virtual bool send(PDU<object> pdu)
        {
            var result = false;
            var pduOut = this.composePdu(pdu);

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
        public virtual PDU<object> receive()
        {
            var result = this.lowerLayer.receive();
            if (result != null)
                return this.decomposePdu(result);

            return null;
        }

        /// <summary>
        //  Compose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        /// <returns></returns>
        protected abstract PDU<object> composePdu(PDU<object> pduInput);

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected abstract PDU<object> decomposePdu(PDU<object> pduInput);
    }
}