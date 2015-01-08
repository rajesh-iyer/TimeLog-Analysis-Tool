using TimeLogManager;
using System.Linq;

namespace TimeLogManager.Analyzers
{
    public class PBIUnitTestingTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, ProductBacklogItem, true) == 0)
            {
                const string UnitTestingTaskTitle = "UNIT TESTING";
                //get atleast one "UNIT TESTING" task assigned to ME
                var unitTestTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(UnitTestingTaskTitle) >= 0).ToList();
                if (unitTestTasks.Count > 0)
                {
                    unitTestTasks.ForEach(p => p.Owner = TaskOwnerType.Developer);
                }
                else
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = "Unit testing task not created yet.",
                        AssignedTo = workitem.AssignedTo
                    });
                }
            }
        }
    }
}