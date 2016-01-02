using System;
using System.Windows;

namespace Arctos.Game
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            
            string comPort = "COM1";
            for (int i = 0; i != e.Args.Length; ++i)
            {
                comPort = e.Args[0];
            }

            ControlUnitApp controlUnitVM = new ControlUnitApp(comPort);
            window.DataContext = controlUnitVM;

            window.Show();
        }
    }
}
