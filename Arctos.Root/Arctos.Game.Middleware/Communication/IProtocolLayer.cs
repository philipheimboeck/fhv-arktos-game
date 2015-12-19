namespace ArctosGameServer.Communication
{
    public interface IProtocolLayer
    {
        bool send(PDU pdu);
        bool receive(PDU pdu);
    }
}