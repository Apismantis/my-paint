using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ShapeLib
{
    public class TRectangle : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Rectangle rect = new Rectangle();
            drawNewShape(isShiftKeyPress, rect, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Rectangle rect = new Rectangle();
            updateShape(isShiftKeyPress, rect, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Rectangle rect = (Rectangle) cc.Content;
            updateStyle(rect);
        }
    }
}
