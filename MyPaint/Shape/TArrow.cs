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
    public class TArrow : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Polygon arrowPolygon = new Polygon();
            arrowPolygon.Points = createArrowPointCollection();
            drawNewShape(isShiftKeyPress, arrowPolygon, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Polygon arrowPolygon = new Polygon();
            arrowPolygon.Points = createArrowPointCollection();
            updateShape(isShiftKeyPress, arrowPolygon, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Polygon arrowPolygon = (Polygon)cc.Content;
            updateStyle(arrowPolygon);
        }

        PointCollection createArrowPointCollection()
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

            Point P2 = new Point();
            Point P3 = new Point();
            Point P5 = new Point();
            Point P6 = new Point();

            Point P1 = new Point(x1, (maxY - minY) / 3 + minY);
            Point P4 = new Point(x2, (maxY - minY) / 2 + minY);
            Point P7 = new Point(x1, 2 * (maxY - minY) / 3 + minY);

            if (x1 < x2)
            {
                P2 = new Point(2 * (maxX - minX) / 3 + minX, (maxY - minY) / 3 + minY);
                P3 = new Point(2 * (maxX - minX) / 3 + minX, minY);
                P5 = new Point(2 * (maxX - minX) / 3 + minX, maxY);
                P6 = new Point(2 * (maxX - minX) / 3 + minX, 2 * (maxY - minY) / 3 + minY);
            }
            else // Người dùng vẽ ngược lên trên trái hoặc trên phải
            {
                P2 = new Point(1 * (maxX - minX) / 3 + minX, (maxY - minY) / 3 + minY);
                P3 = new Point(1 * (maxX - minX) / 3 + minX, minY);
                P5 = new Point(1 * (maxX - minX) / 3 + minX, maxY);
                P6 = new Point(1 * (maxX - minX) / 3 + minX, 2 * (maxY - minY) / 3 + minY);
            }

            pointCollection.Add(P1);
            pointCollection.Add(P2);
            pointCollection.Add(P3);
            pointCollection.Add(P4);
            pointCollection.Add(P5);
            pointCollection.Add(P6);
            pointCollection.Add(P7);

            return pointCollection;
        }
    }
}
