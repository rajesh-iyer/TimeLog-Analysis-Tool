using ServiceCenter.Framework;

namespace TFSUtilities
{
    public class CustomQueryReporterServiceContext : ServiceContext
    {
        public string Project { get; set; }
        public string IterationPath { get; set; }
        public string CommandName { get; set; }
    }
}
