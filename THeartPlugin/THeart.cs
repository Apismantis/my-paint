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

namespace MyPaint
{
    public class THeart : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Path heart = new Path();
            heart.Data = Geometry.Parse("M 240, 200 A 25, 25 0 0 0 200, 240  C 210, 250 240, 270 240, 270  C 240, 270 260, 260 280, 240  A 25, 25 0 0 0 240, 200");
            drawNewShape(isShiftKeyPress, heart, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Path heart = new Path();
            heart.Data = Geometry.Parse("M 240, 200 A 25, 25 0 0 0 200, 240  C 210, 250 240, 270 240, 270  C 240, 270 260, 260 280, 240  A 25, 25 0 0 0 240, 200");
            updateShape(isShiftKeyPress, heart, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Path heart = (Path)cc.Content;
            updateStyle(heart);
        }

        public override TShape clone()
        {
            return new THeart();
        }

        public override string getShapeName()
        {
            return "Heart";
        }
    }
}