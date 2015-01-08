using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class FunctionalTestingTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            const string FunctionalTestingTaskTitle = "FUNCTIONAL TESTING";
            //get atleast one "FUNCTIONAL TESTING" task assigned to anyone
            var testingTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(FunctionalTestingTaskTitle) >= 0).ToList();
            if (testingTasks.Count > 0)
            {
                testingTasks.ForEach(p => p.Owner = TaskOwnerType.Tester);
            }
            else
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Functional testing task not created yet. Please contact respective QA",
                    AssignedTo = GetAssignedToIfTester(context, workitem.AssignedTo)
                });
            }
        }

        private string GetAssignedToIfTester(TimeLogReportingServiceContext context, string assignedTo)
        {
            if (context.TeamProfiles != null && context.TeamProfiles.Count > 0)
            {
                if (context.TeamProfiles.Where(p => string.Compare(p.Fullname, assignedTo, true) == 0 && string.Compare(p.Role, TaskOwnerType.Tester.ToString(), true) == 0).Any())
                {
                    return assignedTo;
                }
            }
            return string.Empty;
        }
    }
}
