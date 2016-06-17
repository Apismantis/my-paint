using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyPaint
{
    public abstract class Command
    {
        public Command()
        {
            MainWindow.registerMe(this);
        }

        public abstract bool undo();

        public abstract bool redo();
    }
}
