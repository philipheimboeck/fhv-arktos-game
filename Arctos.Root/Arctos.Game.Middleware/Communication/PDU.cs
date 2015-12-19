using System.Security.Policy;

namespace ArctosGameServer.Communication
{
    /// <summary>
    /// Packet Data Unit
    /// </summary>
    public class PDU
    {
        public int Version { get; set; }
        public int KeyLength { get; set; }
        public int DataLength { get; set; }
        public string Key { get; set; }
        public string Data { get; set; }

        public string ComposedData { get; set; }
    }
}