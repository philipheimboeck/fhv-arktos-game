using System.Windows;

namespace Arctos.Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Application CurrentApp
        {
            get
            {
                return Application.Current;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
