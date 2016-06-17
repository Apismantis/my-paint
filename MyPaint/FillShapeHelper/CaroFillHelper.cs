using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MyPaint
{
    public class CaroFillHelper : FillShapeHelper
    {
        public override Brush GetBrush(Brush color)
        {
            DrawingBrush caroBrush = new DrawingBrush();

            GeometryDrawing backgroundSquare = new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 50, 50)));

            GeometryGroup aGeometryGroup = new GeometryGroup();
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0, 0, 25, 25)));
            aGeometryGroup.Children.Add(new RectangleGeometry(new Rect(25, 25, 25, 25)));

            LinearGradientBrush checkerBrush = new LinearGradientBrush();
            checkerBrush.GradientStops.Add(new GradientStop(Colors.Black, 0.0));
            checkerBrush.GradientStops.Add(new GradientStop(Colors.Gray, 1.0));

            GeometryDrawing checkers = new GeometryDrawing(checkerBrush, null, aGeometryGroup);

            DrawingGroup checkersDrawingGroup = new DrawingGroup();
            checkersDrawingGroup.Children.Add(backgroundSquare);
            checkersDrawingGroup.Children.Add(checkers);

            caroBrush.Drawing = checkersDrawingGroup;
            caroBrush.Viewport = new Rect(0, 0, 0.25, 0.25);
            caroBrush.TileMode = TileMode.Tile;

            return caroBrush;
        }
    }
}
