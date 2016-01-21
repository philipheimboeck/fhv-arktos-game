using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace ArctosGameServer.ViewModel
{
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}