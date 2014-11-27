using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TFSUtilities
{
    public class CustomQueryReporterServiceContext : TFSServiceContext
    {
        public string Project { get; set; }
        public string IterationPath { get; set; }
        public string CommandName { get; set; }
    }
}
