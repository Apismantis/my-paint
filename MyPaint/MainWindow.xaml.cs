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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Thuộc tính

        TShape shape;
        Point StartPoint;
        Point EndPoint;
        TShapeCreator ShapeCreator = new TShapeCreator();
        int DrawType = 0;
        int CurrentTool = 0;

        bool isMouseDown = false;
        bool isShiftKeyDown = false;
        bool isStrokeColorPress = false;
        bool isFillColorPress = false;
        bool isSelectionTool = false;
        bool isSelectShape = false;

        RichTextBox rtbText;

        double StrokeThicknessSize;
        Brush StrokeBrush;
        Brush FillBrush;
        DoubleCollection Dashes;

        enum DrawElementType : int
        {
            Shape = 1,
            SelectionTool = 2,
            Text = 3,
            Fill = 4
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            StrokeThicknessSize = 1;
            StrokeBrush = MyColorConverter.convertToSolidColor("#FF22B14C");
            FillBrush = System.Windows.Media.Brushes.Transparent;
            Dashes = new DoubleCollection();
        }


        #region Load form

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += new KeyEventHandler(PaintCanvas_KeyDown);
            this.KeyUp += new KeyEventHandler(PaintCanvas_KeyUp);

            initHandleColorClick();
            initHandleShapeToolClick();

            // Thêm các kiểu brush vào combobox
            PoPulateBorderStyle();

            // Thêm các kích thước font vào combobox
            PopulateFontSize();

            // Thêm các kiểu đổ màu vào combobox
            PopulateFillStyle();

            // Thêm Adorner cho Canvas
            AdornerLayer aLayer = AdornerLayer.GetAdornerLayer(PaintCanvas);
            aLayer.Add(new MyPaint.Adorners.ResizingAdorner(PaintCanvas));
        }

        private void initHandleShapeToolClick()
        {
            btnLineTool.Click += ShapeToolClick;
            btnRectangleTool.Click += ShapeToolClick;
            btnEllipseTool.Click += ShapeToolClick;
            btnArrowTool.Click += ShapeToolClick;
        }

        private void initHandleColorClick()
        {
            btnColorFF00A2E8.Click += ColorClickHandle;
            btnColorFF22B14C.Click += ColorClickHandle;
            btnColorFF3F48CC.Click += ColorClickHandle;
            btnColorFFA349A4.Click += ColorClickHandle;
            btnColorFFED1C24.Click += ColorClickHandle;
            btnColorFFFF7F27.Click += ColorClickHandle;
            btnColorFFFFC90E.Click += ColorClickHandle;
            btnColorNoneColor.Click += ColorClickHandle;
        }

        private void PopulateFillStyle()
        {
            cbFillStyle.Items.Add("Solid Color");
            cbFillStyle.Items.Add("LinearGradient");
            cbFillStyle.Items.Add("RadialGradient");
            cbFillStyle.Items.Add("Black & White Checker");
            cbFillStyle.Items.Add("Fill By Image");
            cbFillStyle.SelectedIndex = 0;
        }

        private void PopulateFontSize()
        {
            for (int i = 8; i < 80; i = i + 2)
            {
                cbSizeText.Items.Add(i);
            }
            cbSizeText.SelectedIndex = 2; // Size = 12
        }

        private void PoPulateBorderStyle()
        {
            cbBorderStyle.Items.Add("Straight");
            cbBorderStyle.Items.Add("Dot");
            cbBorderStyle.Items.Add("Dash");
            cbBorderStyle.SelectedIndex = 0;
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

            // Bỏ chọn adorner cho các shape
            unSelectedTheLastChildrenOfCanvas();

            // Không cho sửa text
            MakeRichTextBoxReadOnly();

            // Loại bỏ contentcontrol của shape đang chọn
            if (isSelectShape)
            {
                var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                ContentControl cc = (ContentControl)control;
                cc.Style = null;
            }

            // Xóa bỏ vùng chọn của selectiontool
            if (isSelectionTool)
                PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);

            // Khởi tạo vẽ hình
            if (shape != null && DrawType == (int)DrawElementType.Shape)
                initShape();

            isSelectShape = false;
            isSelectionTool = false;
        }


        // MouseButtonUp Event
        private void PaintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            EndPoint = e.GetPosition(PaintCanvas);

            if (shape != null && StartPoint != EndPoint && DrawType == (int)DrawElementType.Shape)
            {
                shape.removeShape(PaintCanvas.Children);

                shape.EndPoint = EndPoint;
                shape.draw(isShiftKeyDown, PaintCanvas.Children);

                // Đang chọn shape
                isSelectShape = true;
                selectedTheLastChildrenOfCanvas();

                // Shape này là SelectionTool
                isSelectionTool = (CurrentTool == (int)DrawElementType.SelectionTool) ? true : false;
            }

            if (DrawType == (int)DrawElementType.Text)
                InsertNewRichTextBox();
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


        private void initShape()
        {
            shape.StrokeColorBrush = StrokeBrush;
            shape.StrokeThickness = StrokeThicknessSize;
            shape.FillColorBrush = FillBrush;
            shape.StartPoint = StartPoint;
            shape.StrokeType = new DoubleCollection(Dashes);
            Style controlStyle = (Style)FindResource("DesignerItemStyle");

            // Selection tool
            if (CurrentTool == (int)DrawElementType.SelectionTool)
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
        }

        private void MakeRichTextBoxReadOnly()
        {
            if (rtbText != null)
            {
                rtbText.BorderThickness = new Thickness(0);
                rtbText.IsReadOnly = true;
                rtbText.IsDocumentEnabled = false;
                rtbText.Cursor = Cursors.None;
            }
        }

        private void InsertNewRichTextBox()
        {
            int fontSize = int.Parse(cbSizeText.Text);
            string fontFamilies = cbFontText.Text;

            rtbText = new RichTextBox()
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

        #region Set border brush and border thickness for button

        public void setButtonBorderAndThickness(Button button, SolidColorBrush brushColor, double thicknessSize)
        {
            button.BorderBrush = brushColor;
            button.BorderThickness = new Thickness(thicknessSize);
        }

        #endregion

        private void ShapeToolClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn == btnLineTool)
                shape = ShapeCreator.createNewShape("TLine");

            else if (btn == btnArrowTool)
                shape = ShapeCreator.createNewShape("TArrow");

            else if (btn == btnRectangleTool)
                shape = ShapeCreator.createNewShape("TRectangle");

            else if (btn == btnEllipseTool)
                shape = ShapeCreator.createNewShape("TEllipse");

            DrawType = (int)DrawElementType.Shape;
            CurrentTool = (int)DrawElementType.Shape;
        }

        private void ColorClickHandle(object sender, RoutedEventArgs e)
        {
            SolidColorBrush color = null;
            Button btn = (Button)sender;

            if (btn == btnColorNoneColor)
                color = MyColorConverter.convertToSolidColor("#00FFFFFF");

            if (btn == btnColorFF00A2E8)
                color = MyColorConverter.convertToSolidColor("#FF00A2E8");

            if (btn == btnColorFFA349A4)
                color = MyColorConverter.convertToSolidColor("#FFA349A4");

            if (btn == btnColorFFFF7F27)
                color = MyColorConverter.convertToSolidColor("#FFFF7F27");

            if (btn == btnColorFFED1C24)
                color = MyColorConverter.convertToSolidColor("#FFED1C24");


            if (btn == btnColorFF3F48CC)
                color = MyColorConverter.convertToSolidColor("#FF3F48CC");

            if (btn == btnColorFF22B14C)
                color = MyColorConverter.convertToSolidColor("#FF22B14C");


            if (btn == btnColorFFFFC90E)
                color = MyColorConverter.convertToSolidColor("#FFFFC90E");

            if (color != null)
                updateFillStrokeColor(color);
        }

        public void updateFillStrokeColor(Brush color)
        {
            if (isStrokeColorPress)
            {
                btnStrokeColor.Background = color;
                colorPicker.SelectedColor = (StrokeBrush as SolidColorBrush).Color;
                StrokeBrush = color;

                // Cập nhật màu nếu đang chọn shape
                if (isSelectShape)
                    shape.StrokeColorBrush = StrokeBrush;
            }
            else if (isFillColorPress)
            {
                // Thay đổi thuộc tính fill thành solid color
                cbFillStyle.SelectedIndex = 0;
                btnFillColor.Background = color;
                colorPicker.SelectedColor = (FillBrush as SolidColorBrush).Color;

                FillBrush = color;

                // Cập nhật màu nếu đang chọn shape
                if (isSelectShape)
                    shape.FillColorBrush = FillBrush;
            }

            // Update shape, text color
            if (isSelectShape)
                shape.updateShapeStyle(PaintCanvas.Children);
            else
                changeColorText(color);

        }

        // ColorPicker Event
        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SolidColorBrush newColor = new SolidColorBrush((Color)colorPicker.SelectedColor);
            updateFillStrokeColor(newColor);
        }

        // StrokeColor Button Click
        private void btnStrokeColor_Click(object sender, RoutedEventArgs e)
        {
            setButtonBorderAndThickness(btnStrokeColor, MyColorConverter.convertToSolidColor("#000000"), 2);
            setButtonBorderAndThickness(btnFillColor, MyColorConverter.convertToSolidColor("#000000"), 0.3);

            isStrokeColorPress = true;
            isFillColorPress = false;
        }

        // FillColor Button Click
        private void btnFillColor_Click(object sender, RoutedEventArgs e)
        {
            setButtonBorderAndThickness(btnStrokeColor, MyColorConverter.convertToSolidColor("#000000"), 0.3);
            setButtonBorderAndThickness(btnFillColor, MyColorConverter.convertToSolidColor("#000000"), 2);

            isStrokeColorPress = false;
            isFillColorPress = true;
        }

        #region Stroke

        // Đặt kích thước stroke
        public void setStrokeThickness(double strokeThicknessSize)
        {
            StrokeThicknessSize = strokeThicknessSize;

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
            Dashes.Clear();
            switch (cbBorderStyle.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    Dashes.Add(0.5);
                    break;
                case 2:
                    Dashes.Add(4);
                    break;
            }

            // Cập nhật brush nếu đang chọn shape
            if (isSelectShape)
            {
                shape.StrokeType = new DoubleCollection(Dashes);
                shape.updateShapeStyle(PaintCanvas.Children);
            }
        }

        // Thay đổi kiểu Fill
        private void cbFillStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFillColorPress)
            {
                FillShapeHelper fillShapeHelper = null;
                switch (cbFillStyle.SelectedIndex)
                {
                    case 0:
                        fillShapeHelper = new SolidFillHelper();
                        break;

                    case 1:
                        fillShapeHelper = new LinearGradientHelper();
                        break;

                    case 2:
                        fillShapeHelper = new RadialGradientHelper();
                        break;

                    case 3:
                        fillShapeHelper = new CaroFillHelper();
                        break;

                    case 4:
                        fillShapeHelper = new ImageFillHelper();
                        break;

                    default:
                        break;
                }

                if (fillShapeHelper != null)
                {
                    Brush color = (SolidColorBrush)btnFillColor.Background;
                    FillBrush = fillShapeHelper.GetBrush(color);
                }

                // Cập nhật shape nếu đang chọn shape
                if (isSelectShape)
                {
                    shape.FillColorBrush = FillBrush;
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

        private void btnSaveImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.FileName = "MyPaint";
            saveDialog.DefaultExt = ".png";
            saveDialog.Filter = "Png Image|*.png|Jpg Image|*.jpg|Bitmap Image|*.bmp";

            if ((bool)saveDialog.ShowDialog())
            {
                String ext = System.IO.Path.GetExtension(saveDialog.FileName);
                bool saved = BitmapHelper.saveCanvasImage(96, ext, saveDialog.FileName, PaintCanvas);

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
            DrawType = (int)DrawElementType.Shape;
            CurrentTool = (int)DrawElementType.SelectionTool;
            shape = ShapeCreator.createNewShape("TRectangle");

            setButtonBorderAndThickness(btnSelect, (SolidColorBrush)(new BrushConverter().ConvertFrom("#CFD8DC")), 1);
            setButtonBorderAndThickness(btnRectangleTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnLineTool, System.Windows.Media.Brushes.Transparent, 0);
            setButtonBorderAndThickness(btnEllipseTool, System.Windows.Media.Brushes.Transparent, 0);
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
                System.Drawing.Bitmap bmp = BitmapHelper.renderCanvasToBitmap(PaintCanvas);
                CroppedBitmap cb = BitmapHelper.createCroppedBitmapImage(bmp, StartPoint, EndPoint);

                if (cb != null)
                    Clipboard.SetImage(cb);
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
                System.Drawing.Bitmap bmp = BitmapHelper.renderCanvasToBitmap(PaintCanvas);
                CroppedBitmap cb = BitmapHelper.createCroppedBitmapImage(bmp, StartPoint, EndPoint);

                if (cb != null)
                    Clipboard.SetImage(cb);

                PaintCanvas.Children.Add(rect);
            }
            // Thoát chế độ Select
            isSelectionTool = false;
        }

        #endregion

        #region Text

        private void btnInsertText_Click(object sender, RoutedEventArgs e)
        {
            DrawType = (int)DrawElementType.Text;
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
        public void changeColorText(Brush newColor)
        {
            if (isStrokeColorPress)
                changePropertyText(TextElement.ForegroundProperty, newColor);

            if (isFillColorPress)
                changePropertyText(TextElement.BackgroundProperty, newColor);
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

        private void btnLoadShapePlugin_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFill_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}