using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MyPaint
{
    public class TText
    {
        RichTextBox rtbox;
        public double FontSize { get; set; }
        public string FontFamily { get; set; }
        public Point StartPoint { get; set; }
        public Style controlStyle { get; set; }
        public ContentControl cc { get; set; }
        public ContentControl lastCC { get; set; }
        public UIElement DrawElement { get; set; }

        public TText()
        {
            FontFamily = "Arial";
            FontSize = 12;
            StartPoint = new Point(0, 0);

            rtbox = new RichTextBox()
            {
                AcceptsReturn = true,
                IsUndoEnabled = true,
                FontSize = FontSize,
                FontFamily = new FontFamily(FontFamily),
                BorderThickness = new Thickness(0),
                Background = System.Windows.Media.Brushes.Transparent,
                HorizontalContentAlignment = HorizontalAlignment.Stretch
            };
        }

        public virtual void insertNewText(UIElementCollection collection)
        {
            // Create ContentControl
            cc = new ContentControl();
            cc.Width = 400;
            cc.Height = 40;

            Canvas.SetLeft(cc, StartPoint.X);
            Canvas.SetTop(cc, StartPoint.Y);

            cc.Style = controlStyle;
            cc.Content = rtbox;
            DrawElement = cc;

            // Add ContentControl to Canvas
            collection.Add(cc);
        }

        public void MakeTextReadOnly()
        {
            if (rtbox != null)
            {
                rtbox.IsReadOnly = true;
                rtbox.IsDocumentEnabled = false;
                rtbox.Cursor = Cursors.Arrow;
            }
        }

        // Thay đổi 1 loại thuộc tính dp cho văn bản đang chọn
        public void changePropertyText(System.Windows.DependencyProperty dp, object value)
        {
            if (rtbox != null)
            {
                TextSelection textSelection = rtbox.Selection;

                if (!textSelection.IsEmpty)
                {
                    textSelection.ApplyPropertyValue(dp, value);
                    rtbox.Focus();
                }
            }
        }

        // Thay đổi font
        public void changeFontSize(double fontSize)
        {
            this.FontSize = fontSize;
            changePropertyText(RichTextBox.FontSizeProperty, fontSize);
        }

        // Thay đổi kích thước font chữ
        public void changeFontFamily(string fontName)
        {
            this.FontFamily = fontName;
            changePropertyText(RichTextBox.FontFamilyProperty, fontName);
        }

        // Thay đổi màu sắc văn bản, đổ màu văn bản
        public void changeTextColor(bool IsForeground, Brush newColor)
        {
            if (IsForeground)
                changePropertyText(TextElement.ForegroundProperty, newColor);
            else
                changePropertyText(TextElement.BackgroundProperty, newColor);
        }

        public void boldSelectionText()
        {
            FontWeight fontWeight = (FontWeight)rtbox.Selection.GetPropertyValue(RichTextBox.FontWeightProperty);

            if (fontWeight == FontWeights.Bold)
                changePropertyText(RichTextBox.FontWeightProperty, FontWeights.Regular);
            else
                changePropertyText(RichTextBox.FontWeightProperty, FontWeights.Bold);
        }

        public void italicSelectionText()
        {
            FontStyle fontStyle = (FontStyle)rtbox.Selection.GetPropertyValue(RichTextBox.FontStyleProperty);

            if (fontStyle == FontStyles.Italic)
                changePropertyText(RichTextBox.FontStyleProperty, FontStyles.Normal);
            else
                changePropertyText(RichTextBox.FontStyleProperty, FontStyles.Italic);
        }

        public void underlineSelectionText()
        {
            TextDecorationCollection textDecoration = (TextDecorationCollection)rtbox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);

            if (textDecoration.Count == 0)
                changePropertyText(Inline.TextDecorationsProperty, TextDecorations.Underline);
            else
                changePropertyText(Inline.TextDecorationsProperty, null);
        }
    }
}
