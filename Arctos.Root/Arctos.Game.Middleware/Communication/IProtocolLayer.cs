namespace ArctosGameServer.Communication
{
    public interface IProtocolLayer<T, TR>
    {
        bool send(PDU<T> pdu);
        PDU<T> receive();
    }
}