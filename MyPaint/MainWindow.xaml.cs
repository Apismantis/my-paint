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
using System.Windows.Controls.Ribbon;
using Microsoft.Win32;

namespace MyPaint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Thuộc tính

        TShape shape;
        TText TTextBox;
        Point StartPoint;
        Point EndPoint;
        PaintElementCreator ShapeCreator = new TShapeCreator();
        PaintElementCreator TextCreator = new TextCreator();
        int DrawType = 0;
        int CurrentTool = 0;

        bool isMouseDown = false;
        bool isShiftKeyDown = false;
        bool isStrokeColorPress = false;
        bool isFillColorPress = false;
        bool isSelectionTool = false;
        bool isSelectShape = false;
        RibbonButton ActiveButton;

        double StrokeThicknessSize;
        Brush StrokeBrush;
        Brush FillBrush;
        DoubleCollection Dashes;

        enum DrawElementType : int
        {
            Shape = 1,
            SelectionTool = 2,
            Text = 3
        }

        #endregion

        #region Load form

        public MainWindow()
        {
            InitializeComponent();
            StrokeThicknessSize = 1;
            StrokeBrush = MyColorConverter.convertToSolidColor("#FF22B14C");
            FillBrush = System.Windows.Media.Brushes.Transparent;
            Dashes = new DoubleCollection();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.KeyDown += new KeyEventHandler(PaintCanvas_KeyDown);
            this.KeyUp += new KeyEventHandler(PaintCanvas_KeyUp);

            initHandleColorClick();
            initHandleShapeToolClick();
            initHandleClickButton();

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


        private void initHandleClickButton()
        {
            btnLineTool.Click += ButtonOnClick;
            btnRectangleTool.Click += ButtonOnClick;
            btnEllipseTool.Click += ButtonOnClick;
            btnArrowTool.Click += ButtonOnClick;
            btnStar.Click += ButtonOnClick;

            btnSelect.Click += ButtonOnClick;
            btnStrokeColor.Click += ButtonOnClick;
            btnFillColor.Click += ButtonOnClick;

            btnInsertText.Click += ButtonOnClick;
            btnBoldText.Click += ButtonOnClick;
            btnItalicText.Click += ButtonOnClick;
            btnUnderlineText.Click += ButtonOnClick;
        }

        private void initHandleShapeToolClick()
        {
            btnLineTool.Click += ShapeToolClick;
            btnRectangleTool.Click += ShapeToolClick;
            btnEllipseTool.Click += ShapeToolClick;
            btnArrowTool.Click += ShapeToolClick;
            btnStar.Click += ShapeToolClick;
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

        // Mouse Left Down Event
        private void PaintCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            StartPoint = e.GetPosition(PaintCanvas);

            // Bỏ chọn adorner cho các shape
            UnSelectedTheLastChildrenOfCanvas();

            // Ngăn không cho sửa Text
            MakeTextReadOnly();

            // Loại bỏ contentcontrol của shape đang chọn
            if (isSelectShape && PaintCanvas.Children.Count > 0)
            {
                var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                ContentControl cc = (ContentControl)control;
                cc.Style = null;
            }

            // Xóa bỏ vùng chọn của selectiontool
            if (isSelectionTool)
                removeLastChildren();

            // Khởi tạo vẽ hình
            if (shape != null && DrawType == (int)DrawElementType.Shape)
                InitShapeStyle();

            isSelectShape = false;
            isSelectionTool = false;
        }

        // Mouse Move Event
        private void PaintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown && shape != null)
            {
                shape.EndPoint = e.GetPosition(PaintCanvas);
                shape.drawInMouseMove(isShiftKeyDown, PaintCanvas.Children);
            }
        }

        // Mouse Left Up Event
        private void PaintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            EndPoint = e.GetPosition(PaintCanvas);

            // Chèn hình
            if (shape != null && StartPoint != EndPoint && DrawType == (int)DrawElementType.Shape)
            {
                // Xóa bỏ hình đã vé trước đó
                shape.removeShape(PaintCanvas.Children);

                // Vẽ lại hình mới
                shape.EndPoint = EndPoint;
                shape.draw(isShiftKeyDown, PaintCanvas.Children);

                // Đang chọn shape
                isSelectShape = true;
                SelectedTheLastChildrenOfCanvas();

                // Shape này là SelectionTool
                isSelectionTool = (CurrentTool == (int)DrawElementType.SelectionTool) ? true : false;

                // Tạo một ShapeCommand cho Undo, Redo
                if (!isSelectionTool)
                {
                    ShapeCommand cmd = new ShapeCommand(PaintCanvas.Children, PaintCanvas.Children[PaintCanvas.Children.Count - 1]);
                }
            }

            // Chèn văn bản
            if (DrawType == (int)DrawElementType.Text)
                InsertNewTextBox();
        }

        private void InitShapeStyle()
        {
            shape.StrokeColorBrush = StrokeBrush;
            shape.StrokeThickness = StrokeThicknessSize;
            shape.FillColorBrush = FillBrush;
            shape.StartPoint = StartPoint;
            shape.StrokeType = new DoubleCollection(Dashes);
            Style controlStyle = (Style)FindResource("DesignerItemStyle");

            // Khởi tạo cho vùng chọn
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

        private void MakeTextReadOnly()
        {
            if (TTextBox != null)
                TTextBox.MakeTextReadOnly();
        }

        private void InsertNewTextBox()
        {
            TTextBox = TextCreator.CreateNewTextElement();
            TTextBox.StartPoint = EndPoint;
            Style controlStyle = (Style)FindResource("DesignerItemStyle");

            if (controlStyle != null)
                TTextBox.controlStyle = controlStyle;

            TTextBox.insertNewText(PaintCanvas.Children);
            SelectedTheLastChildrenOfCanvas();

            ShapeCommand cmd = new ShapeCommand(PaintCanvas.Children, PaintCanvas.Children[PaintCanvas.Children.Count - 1]);
        }

        #endregion

        #region Handle click event

        private void ButtonOnClick(object sender, RoutedEventArgs e)
        {
            RibbonButton btn = (RibbonButton)sender;
            if (ActiveButton != null)
                SetButtonFocus(ActiveButton, MyColorConverter.convertToSolidColor("#E0E0E0"), 0);

            if (btn == btnLineTool)
                ActiveButton = btnLineTool;

            else if (btn == btnArrowTool)
                ActiveButton = btnArrowTool;

            else if (btn == btnRectangleTool)
                ActiveButton = btnRectangleTool;

            else if (btn == btnEllipseTool)
                ActiveButton = btnEllipseTool;

            else if (btn == btnStar)
                ActiveButton = btnStar;

            else if (btn == btnSelect)
                ActiveButton = btnSelect;

            else if (btn == btnInsertText)
                ActiveButton = btnInsertText;

            else if (btn == btnBoldText)
                ActiveButton = btnBoldText;

            else if (btn == btnItalicText)
                ActiveButton = btnItalicText;

            else if (btn == btnUnderlineText)
                ActiveButton = btnUnderlineText;

            else if (btn == btnStrokeColor)
                ActiveButton = btnStrokeColor;

            else if (btn == btnFillColor)
                ActiveButton = btnFillColor;

            if (ActiveButton != null)
                SetButtonFocus(ActiveButton, MyColorConverter.convertToSolidColor("#90CAF9"), 0.5);
        }

        private void ShapeToolClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            if (btn == btnLineTool)
                shape = ShapeCreator.CreateNewShape("TLine");

            else if (btn == btnArrowTool)
                shape = ShapeCreator.CreateNewShape("TArrow");

            else if (btn == btnRectangleTool)
                shape = ShapeCreator.CreateNewShape("TRectangle");

            else if (btn == btnEllipseTool)
                shape = ShapeCreator.CreateNewShape("TEllipse");

            else if (btn == btnStar)
                shape = ShapeCreator.CreateNewShape("TStar");

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
                UpdateFillStrokeColor(color);
        }

        public void SetButtonFocus(RibbonButton button, SolidColorBrush brushColor, double thicknessSize)
        {
            button.BorderBrush = brushColor;
            button.BorderThickness = new Thickness(thicknessSize);
        }

        #endregion

        #region Load Plugin

        private void btnLoadShapePlugin_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Dll Plugin File|*.dll";

            if ((bool)openFileDialog.ShowDialog())
            {
                TShape plugin = ShapeCreator.LoadShapePlugin(openFileDialog.FileName);
                if (plugin != null)
                {
                    cbShapePlugin.Items.Add(plugin.getShapeName());
                    cbShapePlugin.SelectedIndex = 0;
                }
            }
        }

        private void cbShapePlugin_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string shapeName = cbShapePlugin.SelectedItem.ToString();
            shape = ShapeCreator.CreateNewShape(shapeName);
            DrawType = (int)DrawElementType.Shape;
            CurrentTool = (int)DrawElementType.Shape;
        }

        #endregion

        #region Stroke, Fill color, border thickness

        public void UpdateFillStrokeColor(Brush color)
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
            {
                if (TTextBox != null)
                    TTextBox.changeTextColor(isStrokeColorPress, color);
            }

        }

        // ColorPicker Event
        private void colorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            SolidColorBrush newColor = new SolidColorBrush((Color)colorPicker.SelectedColor);
            UpdateFillStrokeColor(newColor);
        }

        // StrokeColor Button Click
        private void btnStrokeColor_Click(object sender, RoutedEventArgs e)
        {
            isStrokeColorPress = true;
            isFillColorPress = false;
        }

        // FillColor Button Click
        private void btnFillColor_Click(object sender, RoutedEventArgs e)
        {
            isStrokeColorPress = false;
            isFillColorPress = true;
        }

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

        // Tô một vùng cùng màu của bảng vẽ
        private void btnFill_Click(object sender, RoutedEventArgs e)
        {
            // Fill shape
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
            RedoObject.Clear();
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

                shape = ShapeCreator.CreateNewShape("TRectangle");
                shape.FillColorBrush = image;
                shape.StrokeColorBrush = System.Windows.Media.Brushes.Transparent;
                shape.StartPoint = new Point(0, 0);
                shape.EndPoint = new Point(image.ImageSource.Width, image.ImageSource.Height);

                shape.draw(isShiftKeyDown, PaintCanvas.Children);
                ShapeCommand cmd = new ShapeCommand(PaintCanvas.Children, PaintCanvas.Children[PaintCanvas.Children.Count - 1]);
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

        #region Select, Unselect the last children of Canvas

        public void UnSelectedTheLastChildrenOfCanvas()
        {
            if (PaintCanvas.Children.Count >= 1)
            {
                var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                if (control != null)
                    Selector.SetIsSelected(control, false);
            }
        }

        public void SelectedTheLastChildrenOfCanvas()
        {
            if (PaintCanvas.Children.Count >= 1)
            {
                var control = PaintCanvas.Children[PaintCanvas.Children.Count - 1];
                if (control != null)
                    Selector.SetIsSelected(control, true);
            }
        }

        #endregion

        #region Clipboard: Cut, Copy, Paste

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            DrawType = (int)DrawElementType.Shape;
            CurrentTool = (int)DrawElementType.SelectionTool;
            shape = ShapeCreator.CreateNewShape("TRectangle");
        }

        // Copy
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (isSelectionTool)
            {
                // Xóa ContentControl của Select tạo ra
                removeLastChildren();

                // Tạo CroppedBitmap lưu vào Clipboard
                saveCroppedBitmapToClipboard();
            }
        }

        // Paste
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            // Xóa bỏ ContentControl của các hình đang chọn (nếu có)
            UnSelectedTheLastChildrenOfCanvas();

            // Lấy hình ảnh từ Clipboard
            BitmapSource bmi = Clipboard.GetImage();
            if (bmi != null)
            {
                // Tạo ImageBrush để fill cho Rectangle
                ImageBrush imb = new ImageBrush(bmi);

                shape = ShapeCreator.CreateNewShape("TRectangle");
                shape.FillColorBrush = imb;
                shape.StrokeColorBrush = System.Windows.Media.Brushes.Transparent;
                shape.StartPoint = new Point(0, 0);
                shape.EndPoint = new Point(imb.ImageSource.Width, imb.ImageSource.Height);
                Style controlStyle = (Style)FindResource("DesignerItemStyle");
                if (controlStyle != null)
                    shape.controlStyle = controlStyle;

                shape.draw(isShiftKeyDown, PaintCanvas.Children);

                // Show control cho ContentControl
                SelectedTheLastChildrenOfCanvas();
                isSelectShape = true;
                ShapeCommand cmd = new ShapeCommand(PaintCanvas.Children, PaintCanvas.Children[PaintCanvas.Children.Count - 1]);
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
                removeLastChildren();

                shape = ShapeCreator.CreateNewShape("TRectangle");
                shape.StartPoint = StartPoint;
                shape.EndPoint = EndPoint;
                shape.FillColorBrush = System.Windows.Media.Brushes.White;
                shape.StrokeColorBrush = System.Windows.Media.Brushes.Transparent;
                shape.draw(isShiftKeyDown, PaintCanvas.Children);

                // Lưu CroppedBitmapImage vào Clipboard
                saveCroppedBitmapToClipboard();

                ShapeCommand cmd = new ShapeCommand(PaintCanvas.Children, PaintCanvas.Children[PaintCanvas.Children.Count - 1]);
            }

            // Thoát chế độ Select
            isSelectionTool = false;
        }

        private void saveCroppedBitmapToClipboard()
        {
            System.Drawing.Bitmap bmp = BitmapHelper.renderCanvasToBitmap(PaintCanvas);
            CroppedBitmap cb = BitmapHelper.createCroppedBitmapImage(bmp, StartPoint, EndPoint);

            if (cb != null)
                Clipboard.SetImage(cb);
        }

        private void removeLastChildren()
        {
            if (PaintCanvas.Children.Count >= 1)
                PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
        }

        #endregion

        #region Text

        private void btnInsertText_Click(object sender, RoutedEventArgs e)
        {
            DrawType = (int)DrawElementType.Text;
        }

        private void btnBoldText_Click(object sender, RoutedEventArgs e)
        {
            if (TTextBox != null)
                TTextBox.boldSelectionText();
        }

        private void btnItalicText_Click(object sender, RoutedEventArgs e)
        {
            if (TTextBox != null)
                TTextBox.italicSelectionText();
        }

        private void btnUnderlineText_Click(object sender, RoutedEventArgs e)
        {
            if (TTextBox != null)
                TTextBox.underlineSelectionText();
        }

        private void cbFontText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string newFont = cbFontText.SelectedValue.ToString();
            if (TTextBox != null)
                TTextBox.changeFontFamily(newFont);
        }

        private void cbSizeText_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double newSize = double.Parse(cbSizeText.SelectedValue.ToString());
            if (TTextBox != null)
                TTextBox.changeFontSize(newSize);
        }

        #endregion
        
        #region Undo, Redo

        // Earse all children of canvas
        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            PaintCanvas.Children.Clear();
            PaintCanvas.Background = System.Windows.Media.Brushes.White;
            RedoObject.Clear();
            isSelectionTool = false;
            isSelectShape = false;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (isSelectShape)
            {
                PaintCanvas.Children.RemoveAt(PaintCanvas.Children.Count - 1);
                RedoObject.RemoveAt(CurrentUIEIdx);
                CurrentUIEIdx--;
            }
        }

        private static List<Command> RedoObject = new List<Command>();
        private static int CurrentUIEIdx = 0;

        public static void registerMe(Command command)
        {
            //if (CurrentUIEIdx < RedoObject.Count - 1)
            //    RedoObject.RemoveRange(CurrentUIEIdx, RedoObject.Count - CurrentUIEIdx);

            RedoObject.Add(command);
            CurrentUIEIdx = RedoObject.Count - 1;
        }

        // Undo: Xóa các đối tượng con của canvas và thêm vào danh sách các đối tượng redo
        // Redo: Lấy các đối tượng trong danh sách redo thêm vào canvas
        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            CurrentUIEIdx++;
            if (CurrentUIEIdx >= 0 && CurrentUIEIdx < RedoObject.Count)
                RedoObject[CurrentUIEIdx].redo();
            else
                CurrentUIEIdx = RedoObject.Count - 1; // Reset
        }

        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUIEIdx >= 0 && CurrentUIEIdx < RedoObject.Count)
            {
                RedoObject[CurrentUIEIdx].undo();
                CurrentUIEIdx--;
            }

            if (CurrentUIEIdx < 0)
                CurrentUIEIdx = -1;
        }

        #endregion

    }
}