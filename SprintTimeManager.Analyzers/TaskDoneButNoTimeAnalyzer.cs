using TimeLogManager;
using System.Linq;

namespace TimeLogManager.Analyzers
{
    public class TaskDoneButNoTimeAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            var taskDoneButNoTimeQuery = workitem.Tasks.Where(q => q.IsTaskMarkedAsDone == true && q.ActualDevEfforts == 0 && q.Owner != TaskOwnerType.Team);
            if (taskDoneButNoTimeQuery.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Mismatch",
                    Title = "There is atleast one task marked completed, but no time logged under it."
                });
                taskDoneButNoTimeQuery.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Mismatch",
                        Title = string.Format("Task #{0} is marked completed but has no time logged under it.", p.WorkitemId),
                        AssignedTo = p.AssignedTo
                    });
                });
            }

            var incompleteTaskNoRemainingTimeQuery = workitem.Tasks.Where(q => q.RemainingWork == null && q.IsTaskMarkedAsDone == false);
            if (incompleteTaskNoRemainingTimeQuery.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Atleast one of the incomplete task is missing remaining work"
                });
                incompleteTaskNoRemainingTimeQuery.ToList().ForEach(p => {
                    p.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = string.Format("Task #{0} is incomplete but does not have remaining work", p.WorkitemId),
                        AssignedTo = p.AssignedTo
                    });                
                });
            }
        }
    }
}
