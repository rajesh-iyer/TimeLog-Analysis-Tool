using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class ImpactAnalysisTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, ProductBacklogItem, true) == 0)
            {
                //get atleast one "IMPACT ANALYSIS" task assigned to anyone
                const string AnalysisTaskTitle = "IMPACT ANALYSIS";
                var analysisTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(AnalysisTaskTitle) >= 0).ToList();
                if (analysisTasks.Count > 0)
                {
                    analysisTasks.ForEach(p => p.Owner = TaskOwnerType.Team);
                }
                else
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = "Impact Analysis task not created yet",
                        AssignedTo = workitem.AssignedTo
                    });
                }

                //get atleast one "touch points" task assigned to anyone (optional)
                const string TouchPointTaskTitle = "TOUCH POINT";
                var touchPointTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(TouchPointTaskTitle) >= 0).ToList();
                if (touchPointTasks.Count > 0)
                {
                    touchPointTasks.ForEach(p => p.Owner = TaskOwnerType.Team);
                }
            }
        }
    }
}