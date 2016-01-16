using System;
using System.Security.Cryptography.X509Certificates;

namespace ArctosGameServer.Communication
{
    public interface ITcpCommunicator
    {
        int? Read();

        bool Write(byte[] data);
    }
}