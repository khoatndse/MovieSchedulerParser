using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MovieSchedulerParser
{
    /// <summary>
    /// Write Log File
    /// </summary>
    class Logger
    {
        private static Logger instance;
        private static Object Locker = new Object();
        private static string logname = "ParserLog.doc";
        private string CurrentDirectory;
        private Logger()
        {
            CurrentDirectory = Environment.CurrentDirectory.ToString();
        }


        public static void TearDown()
        {
            instance = null;
        }

        public static Logger GetInstance()
        {
            lock (Locker)
            {
                if (instance == null)
                {
                    instance = new Logger();
                    logname = GetLogName()+".txt";
                }
            }
            return instance;
        }

        public void WriteLog(string s)
        {
            StreamWriter logFile = new System.IO.StreamWriter(CurrentDirectory + "\\" + logname, true);
            DateTime now = DateTime.Now;
            String time = now.ToShortDateString() + "-" + now.ToLongTimeString();
            logFile.WriteLine(time + " : " + s);
            logFile.Close();

        }
        public static string GetLogName()
        {
            string s = "";
            DateTime now = DateTime.Now;
            s = now.ToShortDateString() + now.ToLongTimeString();
            s= s.Replace("/","").Replace(":","");
            return s;
        }
    }
}
