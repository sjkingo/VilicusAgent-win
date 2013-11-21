using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VilicusAgent
{
    class Log
    {
        public string filename;

        private StreamWriter _log;        
        private bool _isInteractive;
        private bool _debugFlag;

        public Log(string filename, bool isInteractive, bool debugFlag)
        {
            this.filename = filename;
            this._log = new StreamWriter(filename);
            this._isInteractive = isInteractive;
            this._debugFlag = debugFlag;
        }

        private void _WriteLog(string line, bool stderr)
        {
            _log.WriteLine(line);
            _log.Flush();
            if (_isInteractive)
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
            if (!_debugFlag) return;
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": DEBUG: " + msg;
            _WriteLog(line, false);
        }

        public void Info(string msg)
        {
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": INFO: " + msg;
            _WriteLog(line, false);
        }

        public void Warn(string msg)
        {
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": WARN: " + msg;
            _WriteLog(line, false);
        }

        public void Error(string msg)
        {
            string now = DateTime.Now.ToString("s"); // s: 2008-06-15T21:15:07
            string line = now + ": ERROR: " + msg;
            _WriteLog(line, true);
        }
    }
}
