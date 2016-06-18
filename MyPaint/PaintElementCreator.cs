using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShapeLib;

namespace MyPaint
{
    public interface PaintElementCreator
    {
        TShape CreateNewShape(string shapeName);

        TShape LoadShapePlugin(string fileName);

        TText CreateNewTextElement();
    }
}
