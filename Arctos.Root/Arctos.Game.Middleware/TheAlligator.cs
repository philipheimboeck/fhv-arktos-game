using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArctosGameServer
{
    /// <summary>
    /// The Alligator is dangerous, but also connects
    /// the game server via bluetooth to the robot
    /// </summary>
    public class TheAlligator
    {
        private SerialPort serialPort;

        public TheAlligator(String portName)
        {
            serialPort = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            //serialPort.
            serialPort.Open();
        }

        public void Send(string data)
        {
            serialPort.Write(data);
            //serialPort.
        }
    }
}
