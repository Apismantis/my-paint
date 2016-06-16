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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Thuộc tính

        TShape shape;
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

        public MainWindow()
        {
            InitializeComponent();
        }


        #region Load form

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += new KeyEventHandler(PaintCanvas_KeyDown);
            this.KeyUp += new KeyEventHandler(PaintCanvas_KeyUp);

            // Thêm các kiểu brush vào combobox
            cbBorderStyle.Items.Add("Straight");
            cbBorderStyle.Items.Add("Dot");
            cbBorderStyle.Items.Add("Dash");
            cbBorderStyle.SelectedIndex = 0;

            // Thêm các kích thước font vào combobox
            for (int i = 8; i < 80; i = i + 2)
            {
                cbSizeText.Items.Add(i);
            }
            cbSizeText.SelectedIndex = 2; // Size = 12

            // Thêm các kiểu đổ màu vào combobox
            cbFillStyle.Items.Add("Solid Color");
            cbFillStyle.Items.Add("LinearGradient");
            cbFillStyle.Items.Add("RadialGradient");
            cbFillStyle.Items.Add("Black & White Checker");
            cbFillStyle.Items.Add("Fill By Image");
            cbFillStyle.SelectedIndex = 0;

            // Thêm Adorner cho Canvas
            AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(PaintCanvas);
            aLayer.Add(new MyPaint.Adorners.ResizingAdorner(PaintCanvas));
        }

        #endregion

        #region Canvas Key Event

        // Shift key is pressed
        private void PaintCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
                isShiftKeyDown = true;
        }

        // Shift key is not pressed
        private void PaintCanvas_KeyUp(object sender, KeyEventArgs e)
        {
            isShiftKeyDown = false;
        }

        #endregion

        #region Canvas Mouse Event

        // MouseButtonDown Event
        private void PaintCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;

            StartPoint = e.GetPosition(PaintCanvas);

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
                unSelectedTheLastChildrenOfCanvas();
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
                PaintCanvas.Children.Add(image);

                return;
            }

            #endregion

            #region Đang chọn vẽ shape

            if (shape != null && drawElementType != (int)DrawElementType.Text && drawElementType != (int)DrawElementType.Fill)
            {
                shape.StrokeBrush = strokeBrush;
                shape.StrokeThickness = strokeThicknessSize;
                shape.FillBrush = fillBrush;
                shape.StartPoint = e.GetPosition(PaintCanvas);
                shape.StrokeDashArray = new DoubleCollection(dashes);
                Style controlStyle = (Style)FindResource("DesignerItemStyle");

                // Selection tool
                if (drawElementType == (int)DrawElementType.SelectionTool)
                {
                    DoubleCollection dashesTemp = new DoubleCollection();
                    dashesTemp.Add(0.5);

                    shape.StrokeBrush = System.Windows.Media.Brushes.Blue;
                    shape.StrokeThickness = 1;
                    shape.FillBrush = System.Windows.Media.Brushes.Transparent;
                    shape.StrokeDashArray = new DoubleCollection(dashesTemp);
                }

                if (controlStyle != null)
                    shape.controlStyle = controlStyle;

                // Bỏ đối tượng đang được hiện anchor
                if (PaintCanvas.Children.Count >= 1)
                {
                    unSelectedTheLastChildrenOfCanvas();

                    if (isSelectShape)
                    {
                        var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                        ContentControl cc = (ContentControl)control;

                        // Không cho thay đổi hình đã vẽ
                        cc.Style = null;
                    }

                    // Xóa bỏ ContentControl nếu đang là SelectionTool
                    if (isSelectionTool)
                        PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
                }
            }

            isSelectShape = false;
            isSelectionTool = false;

            #endregion
        }

        // MouseButtonUp Event
        private void PaintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            EndPoint = e.GetPosition(PaintCanvas);

            #region Vẽ shape

            if (shape != null && StartPoint != EndPoint && drawElementType != (int)DrawElementType.Text && drawElementType != (int)DrawElementType.Fill)
            {
                shape.removeShape(PaintCanvas.Children);

                shape.EndPoint = EndPoint;
                shape.draw(isShiftKeyDown, PaintCanvas.Children);

                // Đang chọn shape
                isSelectShape = true;
                selectedTheLastChildrenOfCanvas();

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

                PaintCanvas.Children.Add(rtbText);
            }

            #endregion
        }

        // MouseMove envent
        private void PaintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && shape != null)
            {
                shape.EndPoint = e.GetPosition(PaintCanvas);
                shape.drawInMouseMove(isShiftKeyDown, PaintCanvas.Children);
            }
        }

        #endregion

        #region Shape Tool Click Event

        // Line tool
        private void btnLineTool_Click(object sender, RoutedEventArgs e)
        {
            shape = new TLine();
            drawElementType = (int)DrawElementType.Line;

            setButtonBorderAndThickness(btnLineTool, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CFD8DC")), 1);
            setButtonBorderAndThickness(btnRectangleTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnEllipseTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnSelect, System.Windows.Media.Brushes.Transparent, 0);
        }

        // Rectangle tool
        private void btnRectangleTool_Click(object sender, RoutedEventArgs e)
        {
            shape = new TRectangle();
            drawElementType = (int)DrawElementType.Rectangle;

            setButtonBorderAndThickness(btnRectangleTool, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CFD8DC")), 1);
            setButtonBorderAndThickness(btnLineTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnEllipseTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnSelect, System.Windows.Media.Brushes.Transparent, 0);
        }

        // Ellipse tool
        private void btnEllipseTool_Click(object sender, RoutedEventArgs e)
        {
            shape = new TEllipse();
            drawElementType = (int)DrawElementType.Ellipse;

            setButtonBorderAndThickness(btnEllipseTool, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CFD8DC")), 1);
            setButtonBorderAndThickness(btnLineTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnRectangleTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnSelect, System.Windows.Media.Brushes.Transparent, 0);
        }

        // Arrow tool
        private void btnArrowTool_Click(object sender, RoutedEventArgs e)
        {
            shape = new TArrow();
            drawElementType = (int)DrawElementType.Arrow;
        }

        // Heart tool
        private void btnHeartTool_Click(object sender, RoutedEventArgs e)
        {
           shape = new THeart();
            drawElementType = (int)DrawElementType.Heart;
        }

        // OvalCallOut tool
        private void btnOvalCalloutTool_Click(object sender, RoutedEventArgs e)
        {
           shape = new TOvalCallout();
            drawElementType = (int)DrawElementType.OvalCallOut;
        }

        // Star tool
        private void btnStarTool_Click(object sender, RoutedEventArgs e)
        {
            //shape = new TStar();
            //drawElementType = (int)DrawElementType.Star;
        }

        #endregion

        #region Set border brush and border thickness for button

        public void setButtonBorderAndThickness(Button button, SolidColorBrush brushColor, double thicknessSize)
        {
            button.BorderBrush = brushColor;
            button.BorderThickness = new Thickness(thicknessSize);
        }

        #endregion

        #region Color

        public void setFillStrokeColor(String colorHex)
        {
            if (isStrokeColorPress == true)
            {
                strokeBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex));

                btnStrokeColor.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex));
                colorPicker.SelectedColor = strokeBrush.Color;

                // Cập nhật màu nếu đang chọn shape
                if (isSelectShape)
                {
                    shape.StrokeBrush = strokeBrush;
                    shape.updateShapeStyle(PaintCanvas.Children);
                }
            }
            else if (isFillColorPress == true)
            {
                // Thay đổi thuộc tính fill thành solid color
                cbFillStyle.SelectedIndex = 0;

                fillBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex));

                btnFillColor.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(colorHex));
                colorPicker.SelectedColor = (fillBrush as SolidColorBrush).Color;

                // Cập nhật màu nếu đang chọn shape
                if (isSelectShape)
                {
                    shape.FillBrush = fillBrush;
                    shape.updateShapeStyle(PaintCanvas.Children);
                }
            }

        }

        // ColorPicker Event
        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SolidColorBrush newColor = new SolidColorBrush((Color)colorPicker.SelectedColor);
            if (isStrokeColorPress == true)
            {
                strokeBrush = newColor;
                btnStrokeColor.Background = newColor;

                // Cập nhật màu nếu đang chọn shape
                if (isSelectShape)
                {
                    shape.StrokeBrush = strokeBrush;
                    shape.updateShapeStyle(PaintCanvas.Children);
                }
            }
            else if (isFillColorPress == true)
            {
                fillBrush = newColor;
                btnFillColor.Background = newColor;

                // Cập nhật màu nếu đang chọn shape
                if (isSelectShape)
                {
                    shape.FillBrush = fillBrush;
                    shape.updateShapeStyle(PaintCanvas.Children);
                }
            }

            changeColorText(newColor);
        }

        // StrokeColor Button Click
        private void btnStrokeColor_Click(object sender, RoutedEventArgs e)
        {
            setButtonBorderAndThickness(btnStrokeColor, (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000")), 2);
            setButtonBorderAndThickness(btnFillColor, (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000")), 0.3);

            isStrokeColorPress = true;
            isFillColorPress = false;
        }

        // FillColor Button Click
        private void btnFillColor_Click(object sender, RoutedEventArgs e)
        {
            setButtonBorderAndThickness(btnStrokeColor, (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000")), 0.3);
            setButtonBorderAndThickness(btnFillColor, (SolidColorBrush)(new BrushConverter().ConvertFrom("#000000")), 2);

            isStrokeColorPress = false;
            isFillColorPress = true;
        }

        private void btnColorNoneColor_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#00FFFFFF");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00FFFFFF"));
            changeColorText(newColor);
        }

        private void btnColorFF00A2E8_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FF00A2E8");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF00A2E8"));
            changeColorText(newColor);
        }

        private void btnColorFFA349A4_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FFA349A4");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFA349A4"));
            changeColorText(newColor);
        }

        private void btnColorFFFF7F27_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FFFF7F27");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFF7F27"));
            changeColorText(newColor);
        }

        private void btnColorFFED1C24_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FFED1C24");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFED1C24"));
            changeColorText(newColor);
        }

        private void btnColorFF3F48CC_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FF3F48CC");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF3F48CC"));
            changeColorText(newColor);
        }

        private void btnColorFF22B14C_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FF22B14C");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF22B14C"));
            changeColorText(newColor);
        }

        private void btnColorFFFFC90E_Click(object sender, RoutedEventArgs e)
        {
            setFillStrokeColor("#FFFFC90E");
            SolidColorBrush newColor = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFC90E"));
            changeColorText(newColor);
        }

        #endregion

        #region Stroke

        // Đặt kích thước stroke
        public void setStrokeThickness(int strokeThicknessSize)
        {
            this.strokeThicknessSize = strokeThicknessSize;

            // Cập nhật kích thước stroke nếu đang chọn shape
            if (isSelectShape)
            {
                shape.StrokeThickness = strokeThicknessSize;
                shape.updateShapeStyle(PaintCanvas.Children);
            }
        }

        private void btnStroke1_Click(object sender, RoutedEventArgs e)
        {
            setStrokeThickness(1);
        }

        private void btnStroke2_Click(object sender, RoutedEventArgs e)
        {
            setStrokeThickness(2);
        }

        private void btnStroke3_Click(object sender, RoutedEventArgs e)
        {
            setStrokeThickness(4);
        }

        private void btnStroke4_Click(object sender, RoutedEventArgs e)
        {
            setStrokeThickness(8);
        }

        #endregion

        #region Border, Fill Style

        // Thay đổi kiể Border
        private void cbBorderStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dashes.Clear();
            switch (cbBorderStyle.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    dashes.Add(0.5);
                    break;
                case 2:
                    dashes.Add(4);
                    break;
            }

            // Cập nhật brush nếu đang chọn shape
            if (isSelectShape)
            {
                shape.StrokeDashArray = new DoubleCollection(dashes);
                shape.updateShapeStyle(PaintCanvas.Children);
            }
        }

        // Thay đổi kiểu Fill
        private void cbFillStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFillColorPress)
            {
                switch (cbFillStyle.SelectedIndex)
                {
                    case 0:
                        fillBrush = (SolidColorBrush)btnFillColor.Background;
                        break;

                    // Linear gradient
                    case 1:
                        fillBrush = (SolidColorBrush)btnFillColor.Background;
                        LinearGradientBrush lgb = new LinearGradientBrush();
                        Color c = (fillBrush as SolidColorBrush).Color;

                        lgb.GradientStops.Add(new GradientStop(c, 1.0));
                        lgb.GradientStops.Add(new GradientStop(Colors.White, 0.0));

                        fillBrush = lgb;
                        break;

                    //Radial gradient
                    case 2:
                        fillBrush = (SolidColorBrush)btnFillColor.Background;
                        RadialGradientBrush rgb = new RadialGradientBrush();
                        Color c1 = (fillBrush as SolidColorBrush).Color;

                        rgb.GradientStops.Add(new GradientStop(c1, 1.0));
                        rgb.GradientStops.Add(new GradientStop(Colors.White, 0.0));

                        fillBrush = rgb;
                        break;

                    // Black and White checker
                    case 3:
                        DrawingBrush myBrush = new DrawingBrush();

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

                        myBrush.Drawing = checkersDrawingGroup;
                        myBrush.Viewport = new Rect(0, 0, 0.25, 0.25);
                        myBrush.TileMode = TileMode.Tile;

                        fillBrush = myBrush;
                        break;

                    // Fill by image
                    case 4:
                        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                        openFileDialog.Filter = "Png Image|*.png|Jpg Image|*.jpg|Bitmap Image|*.bmp";
                        openFileDialog.FileName = "MyPaint";
                        openFileDialog.DefaultExt = ".png";

                        if ((bool)openFileDialog.ShowDialog())
                        {
                            ImageBrush image = new ImageBrush();
                            image.ImageSource = new BitmapImage(new Uri(@openFileDialog.FileName, UriKind.Relative));
                            fillBrush = image;
                        }
                        break;
                }

                // Cập nhật shape nếu đang chọn shape
                if (isSelectShape)
                {
                    shape.FillBrush = fillBrush;
                    shape.updateShapeStyle(PaintCanvas.Children);
                }
            }
        }

        #endregion

        #region File: New, Open, Save Image, Exit

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            PaintCanvas.Children.Clear();
            PaintCanvas.Background = System.Windows.Media.Brushes.White;
            redoObjectList.Clear();
        }

        // Open file
        private void btnOpenImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Png Image|*.png|Jpg Image|*.jpg|Bitmap Image|*.bmp";
            openFileDialog.FileName = "MyPaint";
            openFileDialog.DefaultExt = ".png";

            if ((bool)openFileDialog.ShowDialog())
            {
                ImageBrush image = new ImageBrush();
                image.ImageSource = new BitmapImage(new Uri(@openFileDialog.FileName, UriKind.Relative));

                // Đưa ảnh vào 1 hình chữ nhật
                Rectangle rect = new Rectangle();
                rect.Width = image.ImageSource.Width;
                rect.Height = image.ImageSource.Height;
                rect.Fill = image;

                // Thêm ảnh vào Canvas
                PaintCanvas.Children.Clear();
                PaintCanvas.Children.Add(rect);
            }
        }

        // Save image
        public bool saveCanvasImage(int dpi, string ext, string fileName)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(PaintCanvas);
            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(PaintCanvas);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }
            rtb.Render(dv);

            BitmapEncoder encoder;
            switch (ext.ToLower())
            {
                case ".png":
                    encoder = new PngBitmapEncoder();
                    break;
                case ".jpg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case ".bmp":
                    encoder = new BmpBitmapEncoder();
                    break;
                default:
                    return false;
            }

            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var stm = System.IO.File.Create(fileName))
            {
                encoder.Save(stm);
            }
            return true;
        }

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.FileName = "MyPaint";
            saveDialog.DefaultExt = ".png";
            saveDialog.Filter = "Png Image|*.png|Jpg Image|*.jpg|Bitmap Image|*.bmp";

            if ((bool)saveDialog.ShowDialog())
            {
                String ext = System.IO.Path.GetExtension(saveDialog.FileName);
                bool saved = saveCanvasImage(96, ext, saveDialog.FileName);
                if (saved)
                    System.Windows.MessageBox.Show("Save done!", "Save Image");
                else
                    System.Windows.MessageBox.Show("File extension not available!", "Save Image");
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(1);
        }

        #endregion

        #region Select, Unselect the last children of Canvas, Earse all childrens of canvas

        public void unSelectedTheLastChildrenOfCanvas()
        {
            if (PaintCanvas.Children.Count >= 1)
            {
                var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                if (control != null)
                    Selector.SetIsSelected(control, false);
            }
        }

        public void selectedTheLastChildrenOfCanvas()
        {
            if (PaintCanvas.Children.Count >= 1)
            {
                var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                if (control != null)
                    Selector.SetIsSelected(control, true);
            }
        }

        // Earse all children of canvas
        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            PaintCanvas.Children.Clear();
            PaintCanvas.Background = System.Windows.Media.Brushes.White;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (isSelectShape)
            {
                PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
            }
        }

        #endregion

        #region Clipboard: Cut, Copy, Paste

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            drawElementType = (int)DrawElementType.SelectionTool;
            shape = new TRectangle();

            setButtonBorderAndThickness(btnSelect, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CFD8DC")), 1);
            setButtonBorderAndThickness(btnRectangleTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnLineTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnEllipseTool, System.Windows.Media.Brushes.Transparent, 0);
        }

        public System.Drawing.Bitmap renderCanvasToBitmap()
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(PaintCanvas);
            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(PaintCanvas);
                dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
            }

            rtb.Render(dv);
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));

            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);

                // Tạo một Bitmap
                System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(ms);

                return bmp;
            }
        }

        // Tạo CroppedBitmap
        public CroppedBitmap createCroppedBitmapImage(System.Drawing.Bitmap bmp, Point StartPoint, Point EndPoint)
        {
            // Tạo BitmapSource từ Bitmap
            IntPtr hBitmap = bmp.GetHbitmap();
            BitmapSource bms = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            // Tạo 1 CroppedBitmap được cắt theo vùng chọn
            double width = Math.Abs(StartPoint.X - EndPoint.X);
            double height = Math.Abs(StartPoint.Y - EndPoint.Y);
            CroppedBitmap cb = new CroppedBitmap(bms, new Int32Rect((int)Math.Min(StartPoint.X, EndPoint.X), (int)Math.Min(StartPoint.Y, EndPoint.Y), (int)width, (int)height));

            bmp.Dispose();

            return cb;
        }

        // Copy
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (isSelectionTool)
            {
                // Xóa ContentControl của Select tạo ra
                if (PaintCanvas.Children.Count >= 1)
                {
                    PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
                }

                // Tạo CroppedBitmap lưu vào Clipboard
                System.Drawing.Bitmap bmp = renderCanvasToBitmap();
                CroppedBitmap cb = createCroppedBitmapImage(bmp, StartPoint, EndPoint);
                if (cb != null)
                {
                    Clipboard.SetImage(cb);
                }
            }
        }

        // Paste
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            // Xóa bỏ ContentControl của các hình đang chọn (nếu có)
            unSelectedTheLastChildrenOfCanvas();

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

                cc.Style = (Style)FindResource("DesignerItemStyle");
                cc.Content = rectangle;

                // Thêm ContentControl vào Canvas
                PaintCanvas.Children.Add(cc);

                // Show control cho ContentControl
                selectedTheLastChildrenOfCanvas();
                isSelectShape = true;
            }
            // Thoát chế độ Select
            isSelectionTool = false;
        }

        // Cut
        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            if (isSelectionTool)
            {
                // Xóa ContentControl của Select
                if (PaintCanvas.Children.Count >= 1)
                {
                    PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
                }


                Rectangle rect = new Rectangle();
                rect.Width = Math.Abs(StartPoint.X - EndPoint.X);
                rect.Height = Math.Abs(StartPoint.Y - EndPoint.Y);
                rect.Fill = System.Windows.Media.Brushes.White;
                Canvas.SetLeft(rect, Math.Min(StartPoint.X, EndPoint.X));
                Canvas.SetTop(rect, Math.Min(StartPoint.Y, EndPoint.Y));

                // Lưu CroppedBitmapImage vào Clipboard
                System.Drawing.Bitmap bmp = renderCanvasToBitmap();
                CroppedBitmap cb = createCroppedBitmapImage(bmp, StartPoint, EndPoint);
                if (cb != null)
                {
                    Clipboard.SetImage(cb);
                }

                PaintCanvas.Children.Add(rect);
            }
            // Thoát chế độ Select
            isSelectionTool = false;
        }

        #endregion

        #region Text

        private void btnInsertText_Click(object sender, RoutedEventArgs e)
        {
            drawElementType = (int)DrawElementType.Text;
        }

        // Thay đổi 1 loại thuộc tính dp cho văn bản đang chọn
        public void changePropertyText(System.Windows.DependencyProperty dp, object value)
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
        private void cbFontText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newFont = cbFontText.SelectedValue.ToString();
            changePropertyText(FontFamilyProperty, newFont);
        }

        // Thay đổi kích thước font chữ
        private void cbSizeText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double newSize = double.Parse(cbSizeText.SelectedValue.ToString());
            changePropertyText(FontSizeProperty, newSize);
        }

        // Thay đổi màu sắc văn bản, đổ màu văn bản
        public void changeColorText(SolidColorBrush newColor)
        {
            if (isStrokeColorPress)
            {
                changePropertyText(TextElement.ForegroundProperty, newColor);
            }

            if (isFillColorPress)
            {
                changePropertyText(TextElement.BackgroundProperty, newColor);
            }
        }

        #region Bold, Italic, Underline

        private void btnBoldText_Click(object sender, RoutedEventArgs e)
        {
            FontWeight fontWeight = (FontWeight)rtbText.Selection.GetPropertyValue(FontWeightProperty);
            if (fontWeight == FontWeights.Bold)
                changePropertyText(FontWeightProperty, FontWeights.Regular);
            else
                changePropertyText(FontWeightProperty, FontWeights.Bold);
        }

        private void btnItalicText_Click(object sender, RoutedEventArgs e)
        {
            FontStyle fontStyle = (FontStyle)rtbText.Selection.GetPropertyValue(FontStyleProperty);
            if (fontStyle == FontStyles.Italic)
                changePropertyText(FontStyleProperty, FontStyles.Normal);
            else
                changePropertyText(FontStyleProperty, FontStyles.Italic);
        }

        private void btnUnderlineText_Click(object sender, RoutedEventArgs e)
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
                PaintCanvas.Children.Add(uie);
            }
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (PaintCanvas.Children.Count > 0)
            {
                UIElement uie = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                redoObjectList.Push(uie);
                PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
            }
        }

        #endregion

        #region To loang

        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            drawElementType = (int)DrawElementType.Fill;
        }

        public void fillRow(int x, int y, System.Drawing.Color fillColor, System.Drawing.Color colorOfOriginPixel, System.Drawing.Bitmap bitmap)
        {
            int x1 = x;

            // Tô nửa bên trái của dòng
            while (x1 > 0 && (bitmap.GetPixel(x1, y) == colorOfOriginPixel))
            {
                bitmap.SetPixel(x1, y, fillColor);
                x1--;
            }

            x1 = x + 1;
            // Tô nửa bên phải dòng
            while (x1 < bitmap.Width && (bitmap.GetPixel(x1, y) == colorOfOriginPixel))
            {
                bitmap.SetPixel(x1, y, fillColor);
                x1++;
            }
        }

        public void fillColumn(int x, int y, System.Drawing.Color fillColor, System.Drawing.Color colorOfOriginPixel, System.Drawing.Bitmap bitmap)
        {
            int y1 = y - 1;
            // Tô nửa dưới cột
            while (y1 > 0 && (bitmap.GetPixel(x, y1) == colorOfOriginPixel))
            {
                bitmap.SetPixel(x, y1, fillColor);
                y1--;
            }

            y1 = y + 1;
            // Tô nửa trên cột
            while (y1 < bitmap.Height && (bitmap.GetPixel(x, y1) == colorOfOriginPixel))
            {
                bitmap.SetPixel(x, y1, fillColor);
                y1++;
            }
        }

        public System.Drawing.Bitmap fillBitmap(int x, int y, System.Drawing.Color fillColor, System.Drawing.Color colorOfOriginPixel, System.Drawing.Bitmap bitmap)
        {
            // Nếu màu pixel trùng màu cần fill thì không cần tô
            if (fillColor == colorOfOriginPixel)
                return bitmap;

            int x1 = x - 1;
            int y1 = y;

            #region Chạy nửa dưới y

            while (y1 >= 0 && (bitmap.GetPixel(x, y1) == colorOfOriginPixel))
            {
                fillRow(x, y1, fillColor, colorOfOriginPixel, bitmap);

                // Chạy nửa bên trái y1
                while (x1 > 0 && bitmap.GetPixel(x1, y1) == colorOfOriginPixel)
                {
                    fillColumn(x1, y1, fillColor, colorOfOriginPixel, bitmap);
                    x1--;
                }

                //Chạy nửa bên phải y1
                x1 = x + 1;
                while (x1 < bitmap.Width && (bitmap.GetPixel(x1, y1) == colorOfOriginPixel))
                {
                    fillColumn(x1, y1, fillColor, colorOfOriginPixel, bitmap);
                    x1++;
                }
                y1--;
            }

            #endregion

            #region Chạy nửa trên y

            x1 = x;
            y1 = y + 1;

            while (y1 < bitmap.Height && (bitmap.GetPixel(x, y1) == colorOfOriginPixel))
            {
                fillRow(x, y1, fillColor, colorOfOriginPixel, bitmap);

                // Chạy nửa bên trái y1
                while (x1 > 0 && bitmap.GetPixel(x1, y1) == colorOfOriginPixel)
                {
                    fillColumn(x1, y1, fillColor, colorOfOriginPixel, bitmap);
                    x1--;
                }

                //Chạy nửa bên phải y1
                x1 = x + 1;
                while (x1 < bitmap.Width && (bitmap.GetPixel(x1, y1) == colorOfOriginPixel))
                {
                    fillColumn(x1, y1, fillColor, colorOfOriginPixel, bitmap);
                    x1++;
                }

                y1++;
            }

            #endregion

            return bitmap;
        }

        #endregion
        
    }
}