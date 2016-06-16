using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using ShapeLib;

namespace ShapeLib
{
    public class TLine : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Line line = new Line();
            line.X1 = StartPoint.X;
            line.Y1 = StartPoint.Y;
            line.X2 = EndPoint.X;
            line.Y2 = EndPoint.Y;
            drawNewShape(isShiftKeyPress, line, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Line line = new Line();
            line.X1 = StartPoint.X;
            line.Y1 = StartPoint.Y;
            line.X2 = EndPoint.X;
            line.Y2 = EndPoint.Y;
            updateShape(isShiftKeyPress, line, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Line line = (Line)cc.Content;
            updateStyle(line);
        }

        public override TShape clone()
        {
            return new TLine();
        }

        public override string getShapeName()
        {
            return "TLine";
        }
    }
}
