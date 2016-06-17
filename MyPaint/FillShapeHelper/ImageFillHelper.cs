using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyPaint
{
    public class ImageFillHelper : FillShapeHelper
    {
        public override Brush GetBrush(Brush color)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Png Image|*.png|Jpg Image|*.jpg|Bitmap Image|*.bmp";
            openFileDialog.FileName = "MyPaint";
            openFileDialog.DefaultExt = ".png";

            if ((bool)openFileDialog.ShowDialog())
            {
                try
                {
                    ImageBrush image = new ImageBrush();
                    image.ImageSource = new BitmapImage(new Uri(@openFileDialog.FileName, UriKind.Relative));
                    return image;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error fill shape by image: " + e.ToString());
                }
            }

            return color;
        }
    }
}
