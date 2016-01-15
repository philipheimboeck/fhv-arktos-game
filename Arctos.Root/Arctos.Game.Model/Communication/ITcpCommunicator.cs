using System;
using System.Security.Cryptography.X509Certificates;

namespace ArctosGameServer.Communication
{
    public interface ITcpCommunicator
    {
        char? Read();

        bool Write(string data);
    }
}