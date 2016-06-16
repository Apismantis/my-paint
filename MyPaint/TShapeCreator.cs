using ShapeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPaint
{
    class TShapeCreator
    {
        Dictionary<string, TShape> shapes = new Dictionary<string, TShape>();

        public TShapeCreator()
        {
            PopulateAllShape();
        }

        public void PopulateAllShape()
        {
            TRectangle rect = new TRectangle();
            TEllipse ellipse = new TEllipse();
            TTriangle triangle = new TTriangle();
            TArrow arrow = new TArrow();

            shapes.Add(rect.getShapeName(), rect);
            shapes.Add(ellipse.getShapeName(), ellipse);
            shapes.Add(triangle.getShapeName(), triangle);
            shapes.Add(arrow.getShapeName(), arrow);
        }

        public bool PopulateFromPlugin()
        {
            return true;
        }

        public TShape createNewShape(string shapeName)
        {
            if (IsInShapes(shapeName))
                return shapes[shapeName].clone();

            return null;
        }

        public bool IsInShapes(string shapeName)
        {
            return shapes.ContainsKey(shapeName);
        }
    }
}
