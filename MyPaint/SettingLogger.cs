using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyPaint
{
    public class SettingLogger : Logger
    {
        private SettingLogger()
        {
            FileName = "Settings.txt";
        }

        public static Logger getInstances()
        {
            if (instance == null)
                instance = new SettingLogger();

            return instance;
        }
    }
}
