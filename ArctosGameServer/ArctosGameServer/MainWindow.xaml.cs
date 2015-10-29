using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace ArctosGameServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //DoListen();

        }

        /// <summary>
        /// Simple udp to unity test
        /// </summary>
        private void DoListen()
        {
            UdpClient udpServer = new UdpClient(11000);
            var remoteEP = new IPEndPoint(IPAddress.Any, 11000);

            while (true)
            {
                // player1;left;30%
                var asdf = udpServer.Receive(ref remoteEP);
                Console.Write("receive data from " + remoteEP.ToString());
                //udpServer.Send(new byte[] { 1 }, 1, remoteEP); // reply back
            }
        }
    }
}
