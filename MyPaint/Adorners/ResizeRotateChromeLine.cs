using System.Windows;
using System.Windows.Controls;

namespace DiagramDesigner
{
    public class ResizeRotateChromeLine : Control
    {
        static ResizeRotateChromeLine()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ResizeRotateChromeLine), new FrameworkPropertyMetadata(typeof(ResizeRotateChromeLine)));
        }
    }
}