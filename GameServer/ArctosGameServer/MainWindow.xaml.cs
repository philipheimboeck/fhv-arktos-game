using System;
using System.Collections.Generic;
using System.IO.Ports;
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

            TheAlligator bobTheAlligator = new TheAlligator("COM23");
            bobTheAlligator.Send("testData123");
        }
    }
}
