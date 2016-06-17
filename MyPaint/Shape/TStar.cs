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
    public class TStar : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Polygon star = new Polygon();
            star.Points = createPointCollection();
            drawNewShape(isShiftKeyPress, star, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Polygon star = new Polygon();
            star.Points = createPointCollection();
            updateShape(isShiftKeyPress, star, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Polygon star = (Polygon)cc.Content;
            updateStyle(star);
        }

        PointCollection createPointCollection()
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

            Point P1 = new Point((maxX - minX) / 2 + minX, minY);
            Point P2 = new Point(6.25 * (maxX - minX) / 10 + minX, 4 * (maxY - minY) / 10 + minY);
            Point P3 = new Point(maxX, 4 * (maxY - minY) / 10 + minY);
            Point P4 = new Point(7 * (maxX - minX) / 10 + minX, 6 * (maxY - minY) / 10 + minY);
            Point P5 = new Point(8 * (maxX - minX) / 10 + minX, maxY);
            Point P6 = new Point((maxX - minX) / 2 + minX, 7.5 * (maxY - minY) / 10 + minY);

            Point P7 = new Point(2 * (maxX - minX) / 10 + minX, maxY);
            Point P8 = new Point(3 * (maxX - minX) / 10 + minX, 6 * (maxY - minY) / 10 + minY);
            Point P9 = new Point(minX, 4 * (maxY - minY) / 10 + minY);
            Point P10 = new Point(3.75 * (maxX - minX) / 10 + minX, 4 * (maxY - minY) / 10 + minY);

            pointCollection.Add(P1);
            pointCollection.Add(P2);
            pointCollection.Add(P3);
            pointCollection.Add(P4);
            pointCollection.Add(P5);
            pointCollection.Add(P6);
            pointCollection.Add(P7);
            pointCollection.Add(P8);
            pointCollection.Add(P9);
            pointCollection.Add(P10);

            return pointCollection;
        }

        public override TShape clone()
        {
            return new TStar();
        }

        public override string getShapeName()
        {
            return "TStar";
        }
    }
}
