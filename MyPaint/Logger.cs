using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyPaint
{
    public abstract class Logger
    {
        protected static Logger instance;

        protected static string FileName;

        public virtual string[] readAllLine()
        {
            List<string> result = new List<string>();

            try
            {
                string line = "";
                using (StreamReader sr = new StreamReader(FileName))
                {
                    while ((line = sr.ReadLine()) != null)
                        result.Add(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return result.ToArray();
        }

        public virtual bool writeLine(string data)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FileName, true))
                    sw.WriteLine(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        public virtual bool writeLines(string[] data)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FileName, true))
                {
                    foreach (string line in data)
                        sw.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }

        public virtual bool hasLineInFile(string data)
        {
            try
            {
                string line = "";
                using (StreamReader sr = new StreamReader(FileName))
                {
                    while ((line = sr.ReadLine()) != null)
                        if (line.Equals(data))
                            return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return false;
        }
    }
}
