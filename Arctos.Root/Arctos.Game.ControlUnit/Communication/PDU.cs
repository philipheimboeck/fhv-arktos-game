namespace ArctosGameServer.Communication
{
    /// <summary>
    /// Packet Data Unit
    /// </summary>
    public class PDU<T>
    {
        public T data { get; set; }
        public string Key { get; set; }
    }
}