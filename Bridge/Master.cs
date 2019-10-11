using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WallApp.Bridge
{
    public class Master : IOPipe
    {
        public Process SlaveProcess { get; private set; }

        public Master(string slaveFile)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                FileName = slaveFile,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
            };
            SlaveProcess = Process.Start(startInfo);
            OutputStream = SlaveProcess.StandardInput;
            InputStream = SlaveProcess.StandardOutput;
        }
    }
}
