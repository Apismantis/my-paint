using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ShapeLib
{
    public class TEllipse : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Ellipse ellipse = new Ellipse();
            drawNewShape(isShiftKeyPress, ellipse, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Ellipse ellipse = new Ellipse();
            updateShape(isShiftKeyPress, ellipse, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Ellipse ellipse = (Ellipse)cc.Content;
            updateStyle(ellipse);
        }
    }
}
