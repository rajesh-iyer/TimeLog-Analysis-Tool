using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class EstimatesMissingAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (workitem.PlannedDevEfforts == 0)
            {
                //get first developer
                var assignedToDev = workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer).FirstOrDefault();
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Dev estimates are missing.",
                    AssignedTo = assignedToDev != null ? assignedToDev.AssignedTo : string.Empty
                });
            }
            if (workitem.PlannedQAEfforts == 0)
            {
                //get first QA
                var assignedToQA = workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester).FirstOrDefault();
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "QA estimates are missing.",
                    AssignedTo = assignedToQA != null ? assignedToQA.AssignedTo : string.Empty
                });
            }
        }
    }
}
