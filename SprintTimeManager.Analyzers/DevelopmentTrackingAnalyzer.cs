using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class DevelopmentTrackingAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.IsNullOrEmpty(workitem.DevelopmentTracking) == true)
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Development Tracking value not set.",
                    AssignedTo = workitem.AssignedTo
                });
            }
        }
    }
}
