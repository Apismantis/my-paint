using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace ShapeLib
{
    public class TTriangle : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Polygon trianglePolygon = new Polygon();
            trianglePolygon.Points = createTrianglePointCollection();
            drawNewShape(isShiftKeyPress, trianglePolygon, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Polygon trianglePolygon = new Polygon();
            trianglePolygon.Points = createTrianglePointCollection();
            updateShape(isShiftKeyPress, trianglePolygon, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Polygon trianglePolygon = (Polygon)cc.Content;
            updateStyle(trianglePolygon);
        }

        PointCollection createTrianglePointCollection()
        {
            PointCollection pointCollection = new PointCollection();

            double x1 = StartPoint.X;
            double x2 = EndPoint.X;
            double y1 = StartPoint.Y;
            double y2 = EndPoint.Y;

            double maxX = Math.Max(x1, x2);
            double minX = Math.Min(x1, x2);
            double maxY = Math.Max(y1, y2);
            double minY = Math.Min(y1, y2);

            Point P1 = new Point(minX, minY);
            Point P2 = new Point(maxX, maxY);
            Point P3 = new Point(minX, maxY);


            pointCollection.Add(P1);
            pointCollection.Add(P2);
            pointCollection.Add(P3);

            return pointCollection;
        }

        public override TShape clone()
        {
            return new TTriangle();
        }

        public override string getShapeName()
        {
            return "TTriangle";
        }
    }
}
