using ShapeLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace MyPaint
{
    class PaintCanvasManager
    {
        //#region Thuộc tính

        //TShape shape;
        //Point StartPoint;
        //Point EndPoint;
        //int drawElementType = 0;

        //bool isMouseDown = false;
        //bool isShiftKeyDown = false;
        //bool isStrokeColorPress = false;
        //bool isFillColorPress = false;
        //bool isSelectionTool = false;
        //bool isSelectShape = false;

        //System.Windows.Controls.RichTextBox rtbText;

        //int strokeThicknessSize = 1;
        //SolidColorBrush strokeBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF22B14C"));
        //Brush fillBrush = System.Windows.Media.Brushes.Transparent;
        //DoubleCollection dashes = new DoubleCollection();

        //enum DrawElementType : int { Rectangle = 1, Line = 2, Ellipse = 3, Star = 4, Heart = 5, Arrow = 6, OvalCallOut = 7, SelectionTool = 20, Text = 21, Fill = 30 }

        //#endregion

        private PaintCanvasManager paintCanavasManager = null;
        private Canvas paintCanvas; // PaintCanvas in MainWindows.xml

        protected PaintCanvasManager(Canvas canvas)
        {
            paintCanvas = canvas;
        }

        public PaintCanvasManager getInstances(Canvas canvas)
        {
            if (paintCanavasManager == null)
                paintCanavasManager = new PaintCanvasManager(canvas);

            return paintCanavasManager;
        }

        public void drawShape()
        {

        }

        public void removeShape()
        {

        }

        public void insertText()
        {

        }
    }
}
