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
    class paintCanvasManager
    {
        #region Thuộc tính

        TShape shape;
        TShapeCreator shapeCreator = new TShapeCreator();
        Point StartPoint;
        Point EndPoint;

        int drawElementType = 0;

        bool isMouseDown = false;
        bool isShiftKeyDown = false;
        bool isStrokeColorPress = false;
        bool isFillColorPress = false;
        bool isSelectionTool = false;
        bool isSelectShape = false;

        System.Windows.Controls.RichTextBox rtbText;

        int strokeThicknessSize = 1;
        SolidColorBrush strokeBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF22B14C"));
        Brush fillBrush = System.Windows.Media.Brushes.Transparent;
        DoubleCollection dashes = new DoubleCollection();

        enum DrawElementType : int { Rectangle = 1, Line = 2, Ellipse = 3, Star = 4, Heart = 5, Arrow = 6, OvalCallOut = 7, SelectionTool = 20, Text = 21, Fill = 30 }

        #endregion

        private paintCanvasManager paintCanavasManager = null;
        private Canvas paintCanvas; // paintCanvas in MainWindows.xml

        protected paintCanvasManager(Canvas canvas)
        {
            paintCanvas = canvas;
            // Thêm Adorner cho Canvas
            AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(paintCanvas);
            aLayer.Add(new MyPaint.Adorners.ResizingAdorner(paintCanvas));
        }

        public paintCanvasManager getInstances(Canvas canvas)
        {
            if (paintCanavasManager == null)
                paintCanavasManager = new paintCanvasManager(canvas);

            return paintCanavasManager;
        }

        #region Canvas Mouse Event

        // MouseButtonDown Event
        private void drawShape()
        {
            #region Không cho người dùng sửa text đã chèn vào

            if (rtbText != null)
            {
                rtbText.BorderThickness = new Thickness(0);
                rtbText.IsReadOnly = true;
                rtbText.IsDocumentEnabled = false;
                rtbText.Cursor = Cursors.Arrow;
            }

            #endregion

            #region Chèn văn bản khi đang vẽ shape

            if (drawElementType == (int)DrawElementType.Text && isSelectShape == true)
            {
                unSelectTheLastChildrenOfCanvas();
                isSelectShape = false;
            }

            #endregion

            #region Đang chế độ tô loang

            if (drawElementType == (int)DrawElementType.Fill)
            {
                System.Drawing.Bitmap bmp = renderCanvasToBitmap();

                // Lay mau can fill
                Color c = ((SolidColorBrush)btnFillColor.Background as SolidColorBrush).Color;
                System.Drawing.Color fillColor = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);

                // Lấy màu Pixel gốc
                System.Drawing.Color colorOfOriginPixel = bmp.GetPixel((int)StartPoint.X, (int)StartPoint.Y);

                System.Drawing.Bitmap bitmap = fillBitmap((int)StartPoint.X, (int)StartPoint.Y, fillColor, colorOfOriginPixel, bmp);

                BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

                Image image = new Image();
                image.Source = bms;
                paintCanvas.Children.Add(image);

                return;
            }

            #endregion

            #region Đang chọn vẽ shape

            if (shape != null && drawElementType != (int)DrawElementType.Text && drawElementType != (int)DrawElementType.Fill)
            {
                shape.StrokeColorBrush = strokeBrush;
                shape.StrokeThickness = strokeThicknessSize;
                shape.FillColorBrush = fillBrush;
                shape.StartPoint = e.GetPosition(paintCanvas);
                shape.StrokeType = new DoubleCollection(dashes);
                Style controlStyle = (Style)FindResource("DesignerItemStyle");

                // Selection tool
                if (drawElementType == (int)DrawElementType.SelectionTool)
                {
                    DoubleCollection dashesTemp = new DoubleCollection();
                    dashesTemp.Add(0.5);

                    shape.StrokeColorBrush = System.Windows.Media.Brushes.Blue;
                    shape.StrokeThickness = 1;
                    shape.FillColorBrush = System.Windows.Media.Brushes.Transparent;
                    shape.StrokeType = new DoubleCollection(dashesTemp);
                }

                if (controlStyle != null)
                    shape.controlStyle = controlStyle;

                // Bỏ đối tượng đang được hiện anchor
                if (paintCanvas.Children.Count >= 1)
                {
                    unSelectTheLastChildrenOfCanvas();

                    if (isSelectShape)
                    {
                        var control = paintCanvas.Children[paintCanvas.Children.Count - 1];
                        ContentControl cc = (ContentControl)control;

                        // Không cho thay đổi hình đã vẽ
                        cc.Style = null;
                    }

                    // Xóa bỏ ContentControl nếu đang là SelectionTool
                    if (isSelectionTool)
                        paintCanvas.Children.RemoveAt(paintCanvas.Children.Count - 1);
                }
            }

            isSelectShape = false;
            isSelectionTool = false;

            #endregion
        }

        // MouseButtonUp Event
        private void paintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            EndPoint = e.GetPosition(paintCanvas);

            #region Vẽ shape

            if (shape != null && StartPoint != EndPoint && drawElementType != (int)DrawElementType.Text && drawElementType != (int)DrawElementType.Fill)
            {
                shape.removeShape(paintCanvas.Children);

                shape.EndPoint = EndPoint;
                shape.draw(isShiftKeyDown, paintCanvas.Children);

                // Đang chọn shape
                isSelectShape = true;
                selectTheLastChildrenOfCanvas();

                // Shape này là SelectionTool
                isSelectionTool = (drawElementType == (int)DrawElementType.SelectionTool) ? true : false;
            }

            #endregion

            #region Chèn văn bản

            if (drawElementType == (int)DrawElementType.Text)
            {
                int fontSize = int.Parse(cbSizeText.Text);
                string fontFamilies = cbFontText.Text;

                rtbText = new System.Windows.Controls.RichTextBox()
                {
                    MinHeight = 12,
                    MinWidth = 200,
                    AcceptsReturn = true,
                    IsUndoEnabled = true,
                    FontSize = fontSize,
                    FontFamily = new FontFamily(fontFamilies),
                    BorderThickness = new Thickness(0.5),
                    Background = System.Windows.Media.Brushes.Transparent,
                };

                Canvas.SetLeft(rtbText, StartPoint.X);
                Canvas.SetTop(rtbText, StartPoint.Y);

                paintCanvas.Children.Add(rtbText);
            }

            #endregion
        }

        // MouseMove envent
        private void paintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && shape != null)
            {
                shape.EndPoint = e.GetPosition(paintCanvas);
                shape.drawInMouseMove(isShiftKeyDown, paintCanvas.Children);
            }
        }

        #endregion


        private void createNewShape(string shapeName)
        {
            shape = shapeCreator.createNewShape(shapeName);
            drawElementType = (int)DrawElementType.Line;
        }

        #region Color

        public void updateStrokeColor(SolidColorBrush strokeColor)
        {
            shape.StrokeColorBrush = strokeColor;
            shape.updateShapeStyle(paintCanvas.Children);
        }

        public void updateFillColor(Brush fillColor)
        {
            shape.FillColorBrush = fillColor;
            shape.updateShapeStyle(paintCanvas.Children);
        }

        #endregion

        #region Stroke

        // Đặt kích thước stroke
        public void updateStrokeThickness(double strokeThicknessSize)
        {
            shape.StrokeThickness = strokeThicknessSize;
            shape.updateShapeStyle(paintCanvas.Children);
        }

        #endregion

        #region Border, Fill Style

        // Thay đổi kiểu Border
        private void updateStrokeDash(double dashNumber)
        {
            dashes.Clear();
            dashes.Add(dashNumber);

            shape.StrokeType = new DoubleCollection(dashes);
            shape.updateShapeStyle(paintCanvas.Children);
        }

        public void fillShape(SolidColorBrush color, int fillType)
        {
            FillShapeHelper fillShapeHelper = null;
            switch (fillType)
            {
                case 1:
                    fillShapeHelper = new SolidFillHelper();
                    break;

                case 2:
                    fillShapeHelper = new LinearGradientHelper();
                    break;

                case 3:
                    fillShapeHelper = new RadialGradientHelper();
                    break;

                case 4:
                    fillShapeHelper = new CaroFillHelper();
                    break;

                case 5:
                    fillShapeHelper = new ImageFillHelper();
                    break;

                default:
                    break;
            }

            if (fillShapeHelper != null)
                updateFillColor(fillShapeHelper.GetBrush(color));
        }

        #endregion

        #region File: New, Open, Save Image, Exit

        private void resetCanvas()
        {
            paintCanvas.Children.Clear();
            paintCanvas.Background = System.Windows.Media.Brushes.White;
            redoObjectList.Clear();
        }

        // Open image
        public void openImage(string fileName)
        {
            ImageBrush image = new ImageBrush();
            image.ImageSource = new BitmapImage(new Uri(@fileName, UriKind.Relative));

            // Đưa ảnh vào 1 hình chữ nhật
            Rectangle rect = new Rectangle();
            rect.Width = image.ImageSource.Width;
            rect.Height = image.ImageSource.Height;
            rect.Fill = image;

            // Thêm ảnh vào Canvas
            paintCanvas.Children.Clear();
            paintCanvas.Children.Add(rect);
        }

        // Save image
        public bool saveCanvasImage(int dpi, string ext, string fileName)
        {
            BitmapHelper bmpHelper = new BitmapHelper();
            return bmpHelper.saveCanvasImage(dpi, ext, fileName, paintCanvas);
        }

        #endregion

        #region Select, Unselect the last children of Canvas, Earse all childrens of canvas

        public void unSelectTheLastChildrenOfCanvas()
        {
            if (paintCanvas.Children.Count >= 1)
            {
                var control = paintCanvas.Children[paintCanvas.Children.Count - 1];
                if (control != null)
                    Selector.SetIsSelected(control, false);
            }
        }

        public void selectTheLastChildrenOfCanvas()
        {
            if (paintCanvas.Children.Count >= 1)
            {
                var control = paintCanvas.Children[paintCanvas.Children.Count - 1];
                if (control != null)
                    Selector.SetIsSelected(control, true);
            }
        }

        // Earse all children of canvas
        private void resetCanvas(object sender, RoutedEventArgs e)
        {
            paintCanvas.Children.Clear();
            paintCanvas.Background = System.Windows.Media.Brushes.White;
        }

        private void removeLastChildren()
        {
            if (paintCanvas.Children.Count >= 1)
                paintCanvas.Children.RemoveAt(paintCanvas.Children.Count - 1);
        }

        #endregion

        #region Clipboard: Cut, Copy, Paste


        // Copy
        private void copy()
        {
            // Xóa ContentControl của Select tạo ra
            removeLastChildren();

            // Tạo CroppedBitmap lưu vào Clipboard
            BitmapHelper bmpHelper = new BitmapHelper();
            System.Drawing.Bitmap bmp = bmpHelper.renderCanvasToBitmap(paintCanvas);
            CroppedBitmap cb = bmpHelper.createCroppedBitmapImage(bmp, StartPoint, EndPoint);

            if (cb != null)
                Clipboard.SetImage(cb);
        }

        // Paste
        private void pasteImage()
        {
            // Xóa bỏ ContentControl của các hình đang chọn (nếu có)
            unSelectTheLastChildrenOfCanvas();

            // Lấy hình ảnh từ Clipboard
            BitmapSource bmi = Clipboard.GetImage();
            if (bmi != null)
            {
                // Tạo ImageBrush để fill cho Rectangle
                ImageBrush imb = new ImageBrush(bmi);

                ContentControl cc = new ContentControl();
                Rectangle rectangle = new Rectangle();

                rectangle.Stroke = System.Windows.Media.Brushes.Transparent;
                rectangle.Fill = imb;

                rectangle.IsHitTestVisible = false;
                rectangle.Stretch = System.Windows.Media.Stretch.Fill;

                cc.Width = bmi.Width;
                cc.Height = bmi.Height;
                Canvas.SetLeft(cc, 0);
                Canvas.SetTop(cc, 0);

                cc.Style = (Style)Application.Current.FindResource("DesignerItemStyle");
                cc.Content = rectangle;

                // Thêm ContentControl vào Canvas
                paintCanvas.Children.Add(cc);

                // Show control cho ContentControl
                selectTheLastChildrenOfCanvas();
                isSelectShape = true;
            }
        }

        // Cut
        private void cut()
        {
            // Xóa ContentControl của Select
            removeLastChildren();


            Rectangle rect = new Rectangle();
            rect.Width = Math.Abs(StartPoint.X - EndPoint.X);
            rect.Height = Math.Abs(StartPoint.Y - EndPoint.Y);
            rect.Fill = System.Windows.Media.Brushes.White;
            Canvas.SetLeft(rect, Math.Min(StartPoint.X, EndPoint.X));
            Canvas.SetTop(rect, Math.Min(StartPoint.Y, EndPoint.Y));

            // Lưu CroppedBitmapImage vào Clipboard
            BitmapHelper bmpHelper = new BitmapHelper();
            System.Drawing.Bitmap bmp = bmpHelper.renderCanvasToBitmap(paintCanvas);
            CroppedBitmap cb = bmpHelper.createCroppedBitmapImage(bmp, StartPoint, EndPoint);

            if (cb != null)
                Clipboard.SetImage(cb);

            paintCanvas.Children.Add(rect);
        }

        #endregion

        #region Text

        // Thay đổi 1 loại thuộc tính dp cho văn bản đang chọn
        public void changePropertyText(DependencyProperty dp, object value)
        {
            if (rtbText != null)
            {
                TextSelection textSelection = rtbText.Selection;

                if (!textSelection.IsEmpty)
                {
                    textSelection.ApplyPropertyValue(dp, value);
                    rtbText.Focus();
                }
            }
        }

        // Thay đổi font
        private void changeFontFamily(string newFont)
        {
            changePropertyText(RichTextBox.FontFamilyProperty, newFont);
        }

        // Thay đổi kích thước font chữ
        private void changeFontSize(double newSize)
        {
            changePropertyText(RichTextBox.FontSizeProperty, newSize);
        }

        // Thay đổi màu sắc văn bản, đổ màu văn bản
        public void changeColorText(SolidColorBrush newColor)
        {
            changePropertyText(TextElement.ForegroundProperty, newColor);
        }

        public void changeBackgroundText(SolidColorBrush newColor)
        {
            changePropertyText(TextElement.BackgroundProperty, newColor);
        }

        #region Bold, Italic, Underline

        private void changeBoldeWeight()
        {
            FontWeight fontWeight = (FontWeight)rtbText.Selection.GetPropertyValue(RichTextBox.FontWeightProperty);
            if (fontWeight == FontWeights.Bold)
                changePropertyText(RichTextBox.FontWeightProperty, FontWeights.Regular);
            else
                changePropertyText(RichTextBox.FontWeightProperty, FontWeights.Bold);
        }

        private void changeItalicWeight()
        {
            FontStyle fontStyle = (FontStyle)rtbText.Selection.GetPropertyValue(RichTextBox.FontStyleProperty);
            if (fontStyle == FontStyles.Italic)
                changePropertyText(RichTextBox.FontStyleProperty, FontStyles.Normal);
            else
                changePropertyText(RichTextBox.FontStyleProperty, FontStyles.Italic);
        }

        private void underlineText()
        {
            TextDecorationCollection textDecoration = (TextDecorationCollection)rtbText.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            if (textDecoration.Count == 0)
                changePropertyText(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
                changePropertyText(Inline.TextDecorationsProperty, null);
        }

        #endregion

        #endregion

        #region Undo, Redo

        Stack<Object> redoObjectList = new Stack<Object>();

        // Undo: Xóa các đối tượng con của canvas và thêm vào danh sách các đối tượng redo
        // Redo: Lấy các đối tượng trong danh sách redo thêm vào canvas
        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (redoObjectList.Count > 0)
            {
                UIElement uie = (UIElement)redoObjectList.Pop();
                paintCanvas.Children.Add(uie);
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (paintCanvas.Children.Count > 0)
            {
                UIElement uie = paintCanvas.Children[paintCanvas.Children.Count - 1];
                redoObjectList.Push(uie);
                paintCanvas.Children.RemoveAt(paintCanvas.Children.Count - 1);
            }
        }

        #endregion

    }
}
