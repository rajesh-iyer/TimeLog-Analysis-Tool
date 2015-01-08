using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class PBIArchitectCodeReviewTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, ProductBacklogItem, true) == 0)
            {
                const string ArchitectReviewTaskTitle = "ARCHITECT CODE REVIEW";
                //get atleast one "ARCHITECT CODE REVIEW" task assigned to anyone
                var architectCodeReviewTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(ArchitectReviewTaskTitle) >= 0).ToList();
                if (architectCodeReviewTasks.Count > 0)
                {
                    architectCodeReviewTasks.ForEach(p => p.Owner = TaskOwnerType.Architect);
                }
                else
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = "Architect code review task not created yet. Please contact respective architect/Lead",
                        AssignedTo = workitem.AssignedTo   //TODO: Logic to identify architect
                    });
                }
            }
        }
    }
}
