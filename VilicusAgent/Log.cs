using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class Log
    {
        private string filename;
        private StreamWriter fp;        
        private bool isInteractive;
        private bool debugFlag;

        public Log(bool isInteractive)
        {
            this.filename = ConfigurationManager.AppSettings["logFilename"];
            if (this.filename == null)
            {
                this.filename = "agent.log";
            }

            try
            {
                this.fp = new StreamWriter(filename);
            }
            catch (DirectoryNotFoundException exc)
            {
                Console.Error.WriteLine("FATAL: Could not create log file: " + exc.Message);
                System.Environment.Exit(1);
            }
                
            this.isInteractive = isInteractive;

            if (ConfigurationManager.AppSettings["logDebug"] != null)
            {
                debugFlag = Convert.ToBoolean(ConfigurationManager.AppSettings["logDebug"]);
            }
            else
            {
                debugFlag = false;
            }
        }

        override public string ToString()
        {
            return filename;
        }

        private void WriteLog(string line, bool stderr)
        {
            fp.WriteLine(line);
            fp.Flush();
            if (isInteractive)
            {
                if (stderr)
                {
                    Console.Error.WriteLine(line);
                    Console.Error.Flush();
                }
                else
                {
                    Console.WriteLine(line);
                    Console.Out.Flush();
                }
            }
        }

        public void Debug(string msg)
        {
            if (!debugFlag) return;
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": DEBUG: " + msg;
            WriteLog(line, false);
        }

        public void Info(string msg)
        {
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": INFO: " + msg;
            WriteLog(line, false);
        }

        public void Warn(string msg)
        {
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": WARN: " + msg;
            WriteLog(line, false);
        }

        public void Error(string msg)
        {
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": ERROR: " + msg;
            WriteLog(line, true);
        }
    }
}
