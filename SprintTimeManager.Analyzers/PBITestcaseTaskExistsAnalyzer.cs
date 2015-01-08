using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    //get atleast one "TC Writing" task assigned to anyone
    public class PBITestcaseTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, ProductBacklogItem, true) == 0)
            {
                const string TestcaseWritingTaskTitle = "TC WRITING";
                var testingTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(TestcaseWritingTaskTitle) >= 0).ToList();
                if (testingTasks.Count > 0)
                {
                    testingTasks.ForEach(p => p.Owner = TaskOwnerType.Tester);
                }
                else
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = "TC Writing task not created yet. Please contact respective QA",
                        AssignedTo = workitem.AssignedTo   //TODO: Logic to identify tester
                    });
                }
            }
        }
    }
}