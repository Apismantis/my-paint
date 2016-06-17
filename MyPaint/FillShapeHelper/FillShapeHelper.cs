using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace MyPaint
{
    public abstract class FillShapeHelper
    {
        public abstract Brush GetBrush(Brush color);
    }
}
