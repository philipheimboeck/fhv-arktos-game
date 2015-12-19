using System;

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
        public SessionLayer(IProtocolLayer<object, object> lower)
            : base(lower)
        {
            base.lowerLayer = lower;
        }

        /// <summary>
        /// Compose PDU from given input
        /// </summary>
        /// <param name="pduInput"></param>
        /// <returns></returns>
        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            return pduInput;
        }

        /// <summary>
        /// Decompose PDU
        /// </summary>
        /// <param name="pduInput"></param>
        protected override PDU<object> decomposePdu(PDU<object> pduInput)
        {
            return pduInput;
        }
    }
}