using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Shapes;
using TColorLib;

namespace ShapeLib
{
    public abstract class TShape
    {
        public Point StartPoint { get; set; }

        public Point EndPoint { get; set; }

        public Brush StrokeColorBrush { get; set; }

        public Brush FillColorBrush { get; set; }

        public double StrokeThickness { get; set; }

        public DoubleCollection StrokeType { get; set; }

        public UIElement DrawElement { get; set; }

        public Style controlStyle { get; set; }

        public ContentControl cc { get; set; }

        public ContentControl lastCC { get; set; }

        public TShape()
        {
            StartPoint = new Point(0, 0);
            EndPoint = new Point(0, 0);
            StrokeColorBrush = MyColorConverter.convertToSolidColor("#FF22B14C");
            FillColorBrush = System.Windows.Media.Brushes.Transparent;
            StrokeThickness = 1;
        }

        public TShape(Point startPoint, Point endPoint, Brush strokeColorBrush,
            SolidColorBrush fillColorBrush, DoubleCollection strokeType, double strokeThickness)
        {
            this.StartPoint = startPoint;
            this.EndPoint = endPoint;
            this.StrokeColorBrush = strokeColorBrush;
            this.FillColorBrush = fillColorBrush;
            this.StrokeType = strokeType;
            this.StrokeThickness = strokeThickness;
        }

        public abstract void draw(bool isShiftKeyPress, UIElementCollection collection);

        public abstract void drawInMouseMove(bool isShiftKeyPress, UIElementCollection collection);

        public abstract void updateShapeStyle(UIElementCollection collection);

        public abstract TShape clone();

        public abstract string getShapeName();

        // Ve hinh moi
        public virtual void drawNewShape(bool isShiftKeyPress, Shape shape, UIElementCollection collection)
        {
            shape.Stroke = StrokeColorBrush;
            shape.Fill = FillColorBrush;
            shape.StrokeThickness = StrokeThickness;
            shape.StrokeDashArray = StrokeType;

            double width = Math.Abs(StartPoint.X - EndPoint.X);
            double height = Math.Abs(StartPoint.Y - EndPoint.Y);

            // Shape will fill the object containing it
            shape.IsHitTestVisible = false;
            shape.Stretch = System.Windows.Media.Stretch.Fill;

            // Create ContentControl
            cc = new ContentControl();

            cc.Width = width;
            if (isShiftKeyPress)
                cc.Height = width;
            else
                cc.Height = height;

            Canvas.SetLeft(cc, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(cc, Math.Min(StartPoint.Y, EndPoint.Y));

            cc.Style = controlStyle;
            cc.Content = shape;
            DrawElement = cc;

            // Add ContentControl to Canvas
            collection.Add(cc);
        }

        // Cap hinh dang ve
        public virtual void updateShape(bool isShiftKeyPress, Shape shape, UIElementCollection collection)
        {
            bool addSape = false;
            // Create ContentControl
            if (lastCC == null)
            {
                lastCC = new ContentControl();
                addSape = true;
            }

            shape.Stroke = StrokeColorBrush;
            shape.Fill = FillColorBrush;
            shape.StrokeThickness = StrokeThickness;
            shape.StrokeDashArray = StrokeType;

            double width = Math.Abs(StartPoint.X - EndPoint.X);
            double height = Math.Abs(StartPoint.Y - EndPoint.Y);

            // Rectangle will fill the object containing it
            shape.IsHitTestVisible = false;
            shape.Stretch = System.Windows.Media.Stretch.Fill;

            lastCC.Width = width;
            if (isShiftKeyPress == true)
                lastCC.Height = width;
            else
                lastCC.Height = height;

            Canvas.SetLeft(lastCC, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(lastCC, Math.Min(StartPoint.Y, EndPoint.Y));

            lastCC.Style = controlStyle;
            lastCC.Content = shape;

            if (addSape)
                collection.Add(lastCC);
        }

        // Cap nhat style cua hinh
        public virtual void updateStyle(Shape shape)
        {
            shape.Stroke = StrokeColorBrush;
            shape.StrokeThickness = StrokeThickness;
            shape.StrokeDashArray = StrokeType;
            shape.Fill = FillColorBrush;
        }

        // Xoa hinh
        public void removeShape(UIElementCollection collection)
        {
            if (lastCC != null)
            {
                collection.Remove((Shape)lastCC.Content);
                collection.Remove(lastCC);
                lastCC = null;
            }
        }
    }
}
