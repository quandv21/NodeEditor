using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NodeEditor.Converters
{
    public class BoolToVisibilityConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BackgroundToDisabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;
                // Tăng độ sáng lên một chút và giảm opacity
                var disabledColor = Color.FromArgb(
                    (byte)(color.A * 0.9),   // Giảm opacity 50%
                    (byte)Math.Min(255, color.R + 20),
                    (byte)Math.Min(255, color.G + 20),
                    (byte)Math.Min(255, color.B + 20)
                );
                return new SolidColorBrush(disabledColor);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}