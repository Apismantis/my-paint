using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyPaint
{
    public class MyColorConverter
    {
        public static SolidColorBrush brushToSolidBrush(Brush brush)
        {
            return (SolidColorBrush)brush;
        }

        public static SolidColorBrush convertToSolidColor(string colorHex)
        {
            return (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex));
        }

        public static Color getColorFromBrush(Brush brush)
        {
            return (brush as SolidColorBrush).Color;
        }
    }
}
