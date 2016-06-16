using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MyPaint
{
    public class LinearGradientHelper : FillShapeHelper
    {
        public override Brush GetBrush(SolidColorBrush color)
        {
            LinearGradientBrush lgb = new LinearGradientBrush();
            Color c = color.Color;

            lgb.GradientStops.Add(new GradientStop(c, 1.0));
            lgb.GradientStops.Add(new GradientStop(Colors.White, 0.0));

            return lgb;
        }
    }
}
