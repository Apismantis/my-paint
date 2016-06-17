using ShapeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace MyPaint
{
    public class ShapeCommand : Command
    {
        UIElementCollection UICollection;
        Object UIElement;
        PaintUIElementManager pUIElement;

        public ShapeCommand(UIElementCollection collection, Object uie)
        {
            this.UICollection = collection;
            this.UIElement = uie;
            pUIElement = new PaintUIElementManager();
        }

        public override bool undo()
        {
            return pUIElement.remove(UICollection);
        }

        public override bool redo()
        {
            return pUIElement.addObjectToCanvas(UICollection, UIElement);
        }
    }
}
