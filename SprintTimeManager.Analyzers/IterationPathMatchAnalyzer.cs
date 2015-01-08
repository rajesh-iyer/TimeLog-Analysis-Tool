using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class IterationPathMatchAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            var query = workitem.Tasks.Where(p => p.IterationPath != workitem.IterationPath);
            if (workitem.Tasks == null || query.Any())
            {
                Rolewise(workitem, query, TaskOwnerType.Architect, "architect code review");
                Rolewise(workitem, query, TaskOwnerType.Peer, "peer code review");
                Rolewise(workitem, query, TaskOwnerType.Developer, "development");
                Rolewise(workitem, query, TaskOwnerType.Tester, "testing");
                Rolewise(workitem, query, TaskOwnerType.Team, "generic");
            }
        }

        private static void Rolewise(TimeLog workitem, IEnumerable<TimeLog> query, TaskOwnerType owner, string taskRoleTitle)
        {
            var subQuery = query.Where(p => p.Owner == owner);
            if (subQuery.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = string.Format("Atleast one {0} task's iteration does not match to the current iteration.", taskRoleTitle)
                });

                subQuery.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = string.Format("Task #{0}'s iteration does not match to the current iteration.", p.WorkitemId),
                        AssignedTo = p.AssignedTo,
                        WorkitemId = p.WorkitemId
                    });
                });
            }
        }
    }
}
