using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities
{
    public class SprintTimeReporterServiceContext : TFSServiceContext
    {
        public string IterationPath { get; set; }
        public string Project { get; set; }
        public DateTime SprintEndDate { get; set; }
    }
}
