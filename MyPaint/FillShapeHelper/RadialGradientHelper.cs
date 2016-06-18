using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MyPaint
{
    public class RadialGradientHelper : FillShapeHelper
    {
        public override Brush GetBrush(Brush color)
        {
            RadialGradientBrush rgb = new RadialGradientBrush();
            Color c1 = MyColorConverter.getColorFromBrush(color);

            rgb.GradientStops.Add(new GradientStop(c1, 1.0));
            rgb.GradientStops.Add(new GradientStop(Colors.White, 0.0));

            return rgb;
        }
    }
}
