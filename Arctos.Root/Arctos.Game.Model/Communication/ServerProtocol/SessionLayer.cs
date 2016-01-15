namespace ArctosGameServer.Communication.ServerProtocol
{
    public class SessionLayer : ProtocolLayer
    {
        public SessionLayer(IProtocolLayer<object, object> lower) : base(lower)
        {
        }

        protected override PDU<object> composePdu(PDU<object> pduInput)
        {
            return pduInput;
        }

        protected override PDU<object> decomposePdu(PDU<object> pduInput)
        {
            return pduInput;
        }
    }
}