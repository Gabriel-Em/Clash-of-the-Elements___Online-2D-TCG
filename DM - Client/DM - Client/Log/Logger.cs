using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DM___Client.Log
{
    public class Logger
    {
        private string logPath;

        public Logger()
        {
            logPath = @".\Logs\";
        }

        private void checkDirectoryExists()
        {
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
        }

        private string createNewLogPath()
        {
            return logPath + DateTime.Now.ToString("yyyy_dd_MM__HH_mm_ss_") + Guid.NewGuid().ToString() + "_Crash_Log.txt";
        }

        public void Log(string message)
        {
            checkDirectoryExists();

            StreamWriter file = new StreamWriter(createNewLogPath());
            file.WriteLine(message);
            file.Write(DateTime.Now.ToString());
            file.Close();
        }
    }
}
