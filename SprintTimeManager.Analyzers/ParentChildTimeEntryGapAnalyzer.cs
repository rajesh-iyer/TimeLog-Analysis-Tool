using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class ParentChildTimeEntryGapAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (workitem.ActualDevEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer).Sum(q => q.ActualDevEfforts) != 0)
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Mismatch",
                    Title = "Workitem and task level actual efforts (Dev) do not match.",
                    AssignedTo = GetOwnerName(workitem, TaskOwnerType.Developer)
                });
            }
            if (workitem.PlannedDevEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer).Sum(q => q.PlannedDevEfforts) != 0)
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Mismatch",
                    Title = "Workitem and task level planned efforts (Dev) do not match.",
                    AssignedTo = GetOwnerName(workitem, TaskOwnerType.Developer)
                });
            }

            //Note task level efforts for QA and Dev are all set in the DevEfforts fields
            if (workitem.ActualQAEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester).Sum(q => q.ActualDevEfforts) != 0)
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Mismatch",
                    Title = "Workitem and task level planned efforts (QA) do not match.",
                    AssignedTo = GetOwnerName(workitem, TaskOwnerType.Tester)
                });
            }

            if (workitem.PlannedQAEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester).Sum(q => q.PlannedDevEfforts) != 0)
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Mismatch",
                    Title = "Workitem and task level planned efforts (QA) do not match.",
                    AssignedTo = GetOwnerName(workitem, TaskOwnerType.Tester)
                });
            }
        }

        private static string GetOwnerName(TimeLog workitem, TaskOwnerType owner)
        {
            var record = workitem.Tasks.Where(p => p.Owner == owner).FirstOrDefault();
            
            return (record != null) ? record.AssignedTo : string.Empty;
        }
    }
}
