using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MyPaint
{
    public class PaintUIElementManager
    {
        public bool addObjectToCanvas(UIElementCollection collection, Object obj)
        {
            try
            {
                UIElement uie = (UIElement)obj;
                collection.Add(uie);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool remove(UIElementCollection collection)
        {
            try
            {
                if (collection.Count > 0)
                {
                    collection.RemoveAt(collection.Count - 1);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
