using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class PBIValidStatusAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, ProductBacklogItem, true) == 0)
            {
                if (string.Compare(workitem.State, "Committed", true) != 0 && string.Compare(workitem.State, "Done", true) != 0)
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = "The PBI is not marked Committed or Done.",
                        AssignedTo = workitem.AssignedTo
                    });
                }
            }
        }
    }
}