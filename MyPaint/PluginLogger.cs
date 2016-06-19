using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyPaint
{
    public class PluginLogger : Logger
    {
        private PluginLogger()
        {
            FileName = "Plugin.txt";
        }

        public static Logger getInstances()
        {
            if (instance == null)
                instance = new PluginLogger();

            return instance;
        }
    }
}
