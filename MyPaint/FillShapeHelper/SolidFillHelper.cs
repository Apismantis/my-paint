using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MyPaint
{
    public class SolidFillHelper : FillShapeHelper
    {
        public override Brush GetBrush(SolidColorBrush color)
        {
            return color;
        }
    }
}
