using System.Windows.Media;

namespace NodeEditor.Helpers
{
    public class CBrushConverter
    {
        public static SolidColorBrush Convert(string hexColor)
        {
            return new((Color)ColorConverter.ConvertFromString(hexColor));
        }
    }
}
