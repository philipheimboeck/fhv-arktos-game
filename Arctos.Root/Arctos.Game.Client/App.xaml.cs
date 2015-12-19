using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Arctos.Game.Client.Service;

namespace Arctos.Game.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            GameClientService clientService = new GameClientService();
            clientService.Connect("127.0.0.1", "test");
        }
    }
}
