using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyPaint
{
    public class BitmapHelper
    {
        public static bool saveCanvasImage(int dpi, string ext, string fileName, Canvas paintCanvas)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(paintCanvas);
            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(paintCanvas);
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

        public static System.Drawing.Bitmap renderCanvasToBitmap(Canvas paintCanvas)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(paintCanvas);
            RenderTargetBitmap rtb = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();

            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(paintCanvas);
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
        public static CroppedBitmap createCroppedBitmapImage(System.Drawing.Bitmap bmp, Point StartPoint, Point EndPoint)
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
    }
}
