using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class BugCodingTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, Bug, true) == 0)
            {
                const string CodingTaskTitle = "CODING";
                var unitTestTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(CodingTaskTitle) >= 0).ToList();
                if (unitTestTasks.Count > 0)
                {
                    unitTestTasks.ForEach(p => p.Owner = TaskOwnerType.Developer);
                }
                else
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = "Coding task not created yet.",
                        AssignedTo = GetAssignedToIfDeveloper(context, workitem.AssignedTo)
                    });
                }
            }
        }

        //check if current assigned person is developer
        private static string GetAssignedToIfDeveloper(TimeLogReportingServiceContext context, string assignedTo)
        {
            if (context.TeamProfiles != null && context.TeamProfiles.Count > 0)
            {
                if (context.TeamProfiles.Where(p => string.Compare(p.Fullname, assignedTo, true) == 0 && string.Compare(p.Role, TaskOwnerType.Developer.ToString(), true) == 0).Any())
                {
                    return assignedTo;
                }
            }
            return string.Empty;
        }
    }
}
