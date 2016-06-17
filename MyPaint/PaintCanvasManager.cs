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
using TColorLib;

namespace MyPaint
{
    class PaintCanvasManager
    {
        #region Thuộc tính

        public TShape shape;
        public TShapeCreator shapeCreator = new TShapeCreator();
        public Point StartPoint;
        public Point EndPoint;
        public RichTextBox rtbText = new RichTextBox();

        public double StrokeThicknessSize = 1;
        public SolidColorBrush StrokeBrush = MyColorConverter.convertToSolidColor("#FF22B14C");
        public Brush FillBrush = System.Windows.Media.Brushes.Transparent;
        public DoubleCollection Dashes = new DoubleCollection();
        public int DrawType = 0;
        public int CurrentTool = 0;

        enum DrawElementType : int
        {
            Shape = 1,
            SelectionTool = 2,
            Text = 3,
            Fill = 4
        }

        private static PaintCanvasManager paintCanavasManager = null;
        private Canvas paintCanvas; // paintCanvas in MainWindows.xml

        #endregion


        protected PaintCanvasManager(Canvas canvas)
        {
            paintCanvas = canvas;
            // Thêm Adorner cho Canvas
            //AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(canvas);
            //aLayer.Add(new MyPaint.Adorners.ResizingAdorner(canvas));
        }

        public static PaintCanvasManager getInstances(Canvas canvas)
        {
            if (paintCanavasManager == null)
                paintCanavasManager = new PaintCanvasManager(canvas);

            return paintCanavasManager;
        }

        #region Canvas Mouse Event

        // Bỏ chọn shape, bỏ chỉnh sửa text, vẽ shape
        public void mouseDown(Point startPoint)
        {
            paintCanavasManager.StartPoint = startPoint;

            // Ngăn không cho chỉnh sửa text
            //paintCanavasManager.makeRichBoxReadOnly();

            // Bỏ chọn shape đang vẽ
            paintCanavasManager.unSelectTheLastChildrenOfCanvas();

            // Khởi tạo để vẽ shape mới
            if (shape != null && DrawType == (int)DrawElementType.Shape)
            {
                shape.StrokeColorBrush = StrokeBrush;
                shape.StrokeThickness = StrokeThicknessSize;
                shape.FillColorBrush = FillBrush;
                shape.StartPoint = startPoint;
                shape.StrokeType = new DoubleCollection(Dashes);
                Style controlStyle = (Style)Application.Current.FindResource("DesignerItemStyle");

                // Selection tool
                if (DrawType == (int)DrawElementType.SelectionTool)
                {
                    DoubleCollection dashesTemp = new DoubleCollection();
                    dashesTemp.Add(0.5);

                    shape.StrokeColorBrush = System.Windows.Media.Brushes.Blue;
                    shape.StrokeThickness = 1;
                    shape.FillColorBrush = System.Windows.Media.Brushes.Transparent;
                    shape.StrokeType = new DoubleCollection(dashesTemp);

                    // Xoa bo content control
                    removeLastChildren();
                }

                if (controlStyle != null)
                    shape.controlStyle = controlStyle;
            }


        }

        // MouseButtonUp Event
        public void mouseUp(Point endPoint, bool isShiftKeyDown)
        {

            if (shape != null && StartPoint != endPoint && DrawType == (int)DrawElementType.Shape)
            {
                shape.removeShape(paintCanvas.Children); // Xóa shape cũ, vẽ shape mới

                paintCanavasManager.EndPoint = endPoint;
                shape.EndPoint = endPoint;
                shape.draw(isShiftKeyDown, paintCanvas.Children);
                selectTheLastChildrenOfCanvas();
            }

            #region Chèn văn bản

            //if (drawElementType == (int)DrawElementType.Text)
            //{
            //    int fontSize = int.Parse(cbSizeText.Text);
            //    string fontFamilies = cbFontText.Text;

            //    rtbText = new System.Windows.Controls.RichTextBox()
            //    {
            //        MinHeight = 12,
            //        MinWidth = 200,
            //        AcceptsReturn = true,
            //        IsUndoEnabled = true,
            //        FontSize = fontSize,
            //        FontFamily = new FontFamily(fontFamilies),
            //        BorderThickness = new Thickness(0.5),
            //        Background = System.Windows.Media.Brushes.Transparent,
            //    };

            //    Canvas.SetLeft(rtbText, StartPoint.X);
            //    Canvas.SetTop(rtbText, StartPoint.Y);

            //    paintCanvas.Children.Add(rtbText);
            //}

            #endregion
        }

        // MouseMove envent
        public void mouseMove(Point endPoint, bool isShiftKeyDown)
        {
            if (shape != null)
            {
                shape.EndPoint = endPoint;
                shape.drawInMouseMove(isShiftKeyDown, paintCanvas.Children);
            }
        }

        public void removeEditable()
        {
            var control = paintCanvas.Children[paintCanvas.Children.Count - 1];
            ContentControl cc = (ContentControl)control;

            // Không cho thay đổi hình đã vẽ
            cc.Style = null;
        }

        public void makeRichBoxReadOnly()
        {
            if (rtbText != null)
            {
                rtbText.BorderThickness = new Thickness(0);
                rtbText.IsReadOnly = true;
                rtbText.IsDocumentEnabled = false;
                rtbText.Cursor = Cursors.Arrow;
            }
        }

        #endregion


        public void createNewShape(string shapeName)
        {
            shape = shapeCreator.createNewShape(shapeName);
        }


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

        // Đặt kích thước stroke
        public void updateStrokeThickness(double strokeThicknessSize)
        {
            shape.StrokeThickness = strokeThicknessSize;
            shape.updateShapeStyle(paintCanvas.Children);
        }

        // Thay đổi kiểu Border
        public void updateStrokeDash(double dashNumber)
        {
            Dashes.Clear();
            Dashes.Add(dashNumber);

            shape.StrokeType = new DoubleCollection(Dashes);
            shape.updateShapeStyle(paintCanvas.Children);
        }

        public void changeFillStyle(SolidColorBrush color, int fillStyle)
        {
            FillShapeHelper fillShapeHelper = null;
            switch (fillStyle)
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
        
        public void resetCanvas()
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
        public void resetCanvas(object sender, RoutedEventArgs e)
        {
            paintCanvas.Children.Clear();
            paintCanvas.Background = System.Windows.Media.Brushes.White;
        }

        public void removeLastChildren()
        {
            if (paintCanvas.Children.Count >= 1)
                paintCanvas.Children.RemoveAt(paintCanvas.Children.Count - 1);
        }

        #region Clipboard: Cut, Copy, Paste


        // Copy
        public void copy()
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
        public void pasteImage()
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
                //isSelectShape = true;
            }
        }

        // Cut
        public void cut()
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
        public void changeFontFamily(string newFont)
        {
            changePropertyText(RichTextBox.FontFamilyProperty, newFont);
        }

        // Thay đổi kích thước font chữ
        public void changeFontSize(double newSize)
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

        public void changeBoldeWeight()
        {
            FontWeight fontWeight = (FontWeight)rtbText.Selection.GetPropertyValue(RichTextBox.FontWeightProperty);
            if (fontWeight == FontWeights.Bold)
                changePropertyText(RichTextBox.FontWeightProperty, FontWeights.Regular);
            else
                changePropertyText(RichTextBox.FontWeightProperty, FontWeights.Bold);
        }

        public void changeItalicWeight()
        {
            FontStyle fontStyle = (FontStyle)rtbText.Selection.GetPropertyValue(RichTextBox.FontStyleProperty);
            if (fontStyle == FontStyles.Italic)
                changePropertyText(RichTextBox.FontStyleProperty, FontStyles.Normal);
            else
                changePropertyText(RichTextBox.FontStyleProperty, FontStyles.Italic);
        }

        public void underlineText()
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
        public void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (redoObjectList.Count > 0)
            {
                UIElement uie = (UIElement)redoObjectList.Pop();
                paintCanvas.Children.Add(uie);
            }
        }

        public void btnUndo_Click(object sender, RoutedEventArgs e)
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
