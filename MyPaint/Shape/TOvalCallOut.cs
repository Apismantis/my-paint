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
    public class TOvalCallout : TShape
    {
        public override void draw(bool isShiftKeyPress, UIElementCollection collection)
        {
            Path ovalCallout = new Path();
            ovalCallout.Data = createOvalPointCollection();
            drawNewShape(isShiftKeyPress, ovalCallout, collection);
        }

        public override void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection)
        {
            Path ovalCallout = new Path();
            ovalCallout.Data = createOvalPointCollection();
            updateShape(isShiftKeyPress, ovalCallout, collection);
        }

        public override void updateShapeStyle(UIElementCollection collection)
        {
            Path ovalCallout = (Path)cc.Content;
            updateStyle(ovalCallout);
        }

        PathGeometry createOvalPointCollection()
        {
            PointCollection pointCollection = new PointCollection();
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            double x1 = StartPoint.X;
            double x2 = EndPoint.X;
            double y1 = StartPoint.Y;
            double y2 = EndPoint.Y;

            double maxX = Math.Max(x1, x2);
            double minX = Math.Min(x1, x2);
            double maxY = Math.Max(y1, y2);
            double minY = Math.Min(y1, y2);

            Point P1 = new Point((maxX - minX) / 6 + minX, 6 * (maxY - minY) / 10 + minY);
            Point P2 = new Point(2 * (maxX - minX) / 6 + minX, 7 * (maxY - minY) / 10 + minY);
            Point P3 = new Point(2 * (maxX - minX) / 10 + minX, maxY);

            pathFigure.StartPoint = P1;
            pathFigure.Segments.Add(new ArcSegment(P2, new Size(maxX - minX, maxY - minY), 0, true, SweepDirection.Clockwise, true));
            pathFigure.Segments.Add(new LineSegment(P3, true));
            pathFigure.Segments.Add(new LineSegment(P1, true));

            pathGeometry.Figures.Add(pathFigure);
            return pathGeometry;
        }

        public override TShape clone()
        {
            return new TOvalCallout();
        }

        public override string getShapeName()
        {
            return "TOvalCallOut";
        }
    }
}
