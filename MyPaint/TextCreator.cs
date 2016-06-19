using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShapeLib;

namespace MyPaint
{
    public class TextCreator : PaintElementCreator
    {
        public ShapeLib.TShape CreateNewShape(string shapeName)
        {
            return null;
        }

        public ShapeLib.TShape LoadShapePlugin(string fileName)
        {
            return null;
        }

        public TText CreateNewTextElement()
        {
            return new TText();
        }

        public string[] LoadPluginFromPluginLogFile()
        {
            return null;
        }
    }
}
