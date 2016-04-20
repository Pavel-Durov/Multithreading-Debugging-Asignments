﻿using Assignments.Core.Handlers;
using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Consumer.Global
{
    class DumpFile
    {
        const int PID_NOT_FOUND = -1;
        const int ATTACH_TO_PPROCESS_TIMEOUT = 999999;

        internal static void DoAnaytics(string dumpFile)
        {
            using (DataTarget target = DataTarget.LoadCrashDump(dumpFile)) 
            {
                ClrRuntime runtime = target.ClrVersions[0].CreateRuntime();

                //Dump process handler
                ThreadStackHandler handler = new ThreadStackHandler(target.DebuggerInterface, runtime, dumpFile);

                var result = handler.Handle();

                PrintHandler.Print(result, true);
            }
        }

        private static int GetPidFromDumpProcessTextFile()
        {
            int pid = PID_NOT_FOUND;

            var fileContent = File.ReadAllText(@"./../../../dump_pid.txt");
            if (!String.IsNullOrEmpty(fileContent))
            {
                var success = int.TryParse(fileContent, out pid);
            }

            return pid;
        }

     

     
       
    }
}
