using System.Windows;

namespace Arctos.Game
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// App startup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            string comPort = "COM1";
            for (int i = 0; i != e.Args.Length; ++i)
            {
                comPort = e.Args[0];
            }

            MainClass process = new MainClass(comPort);
            process.Start();
        }
    }
}
