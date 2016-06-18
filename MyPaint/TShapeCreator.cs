using ShapeLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyPaint
{
    class TShapeCreator : PaintElementCreator
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
            TStar star = new TStar();
            TLine line = new TLine();
            TArrow arrow = new TArrow();

            shapes.Add(rect.getShapeName(), rect);
            shapes.Add(ellipse.getShapeName(), ellipse);
            shapes.Add(star.getShapeName(), star);
            shapes.Add(arrow.getShapeName(), arrow);
            shapes.Add(line.getShapeName(), line);
        }

        public bool PopulateFromPlugin()
        {
            return true;
        }

        public TShape CreateNewShape(string shapeName)
        {
            if (IsInShapes(shapeName))
                return shapes[shapeName].clone();

            return null;
        }

        public bool IsInShapes(string shapeName)
        {
            return shapes.ContainsKey(shapeName);
        }

        public TShape LoadShapePlugin(string fileName)
        {
            TShape result = null;
            try
            {
                Assembly asm = Assembly.LoadFile(fileName);
                result = CreatePluginFromAssembly(asm);
                shapes.Add(result.getShapeName(), result);
            }
            catch (Exception)
            {
                return null;
            }
            return result;
        }

        public TShape CreatePluginFromAssembly(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
            {
                TShape result = CreateFromType(type);
                if (result != null)
                    return result;
            }
            return null;
        }

        public TShape CreateFromType(Type type)
        {
            try
            {
                TShape result = (TShape)Activator.CreateInstance(type);
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public TText CreateNewTextElement()
        {
            return null;
        }
    }
}
