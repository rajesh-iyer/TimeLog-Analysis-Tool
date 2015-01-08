using ServiceCenter.Framework;
using System;

namespace TimeLogManager
{
    public abstract class TimeLogAnalyzer : Plugin, IComparable<TimeLogAnalyzer>
    {
        protected const string ProductBacklogItem = "Product Backlog Item";
        protected const string Bug = "Bug";

        public int Sequence { get; set; }
        public abstract void Analyze(TimeLogReportingServiceContext context, TimeLog workitem);

        public TimeLogAnalyzer()
        {
            Sequence = 1000;
            PluginType = typeof(TimeLogAnalyzer).FullName;
        }

        public int CompareTo(TimeLogAnalyzer other)
        {
            if (other == null) return 1;

            return this.Sequence.CompareTo(other.Sequence);
        }
    }
}
