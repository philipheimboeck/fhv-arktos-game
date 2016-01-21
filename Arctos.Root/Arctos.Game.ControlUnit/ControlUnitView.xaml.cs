using System.Windows;

namespace Arctos.Game.ControlUnit
{
    /// <summary>
    /// Interaction logic for ControlUnitView.xaml
    /// </summary>
    public partial class ControlUnitView : Window
    {
        public ControlUnitView()
        {
            InitializeComponent();

            LogBox.TextChanged += (sender, args) =>
            {
                LogBox.CaretIndex = LogBox.Text.Length;
                LogBox.ScrollToEnd();
            }; 
        }
    }
}