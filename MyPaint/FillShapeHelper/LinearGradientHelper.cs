using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using TColorLib;

namespace MyPaint
{
    public class LinearGradientHelper : FillShapeHelper
    {
        public override Brush GetBrush(Brush color)
        {
            LinearGradientBrush lgb = new LinearGradientBrush();
            Color c = MyColorConverter.getColorFromBrush(color);

            lgb.GradientStops.Add(new GradientStop(c, 1.0));
            lgb.GradientStops.Add(new GradientStop(Colors.White, 0.0));

            return lgb;
        }
    }
}
