﻿using System.Windows;

namespace Arctos.Game
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var window = new MainWindow();

            var comPort = "COM1";
            for (var i = 0; i != e.Args.Length; ++i)
            {
                comPort = e.Args[0];
            }

            var controlUnitVM = new ControlUnitApp(comPort);
            window.DataContext = controlUnitVM;

            window.Show();
        }
    }
}