using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class ActivityTypeAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            //development tasks
            var query = workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer && string.Compare(p.Activity, "Development", true) != 0);
            if (query.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "One or more developer tasks do not have the right \"Activity\" (Development)."
                });

                query.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = string.Format("Developer task #{0} does not have the right \"Activity\" (Development).", p.WorkitemId),
                        AssignedTo = p.AssignedTo,
                        WorkitemId = p.WorkitemId
                    });
                });
            }

            //QA tasks
            query = workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester && string.Compare(p.Activity, "Testing", true) != 0);
            if (query.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "One or more tester tasks do not have the right \"Activity\" (Testing)."
                });

                query.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = string.Format("Testing task #{0} does not have the right \"Activity\" (Testing).", p.WorkitemId),
                        AssignedTo = p.AssignedTo,
                        WorkitemId = p.WorkitemId
                    });
                });
            }

            //peer review task
            query = workitem.Tasks.Where(p => (p.Owner == TaskOwnerType.Peer) && string.Compare(p.Activity, "Design", true) != 0);
            if (query.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Peer review task does not have the right \"Activity\" (Design)."
                });
                query.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = string.Format("Peer review task #{0} does not have the right \"Activity\" (Design).", p.WorkitemId),
                        AssignedTo = p.AssignedTo,
                        WorkitemId = p.WorkitemId
                    });
                });
            }

            //architect review task
            query = workitem.Tasks.Where(p => (p.Owner == TaskOwnerType.Architect) && string.Compare(p.Activity, "Design", true) != 0);
            if (query.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Architect review task does not have the right \"Activity\" (Design)."
                });
                query.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Lifecycle checklist",
                        Title = string.Format("Architect review task #{0} does not have the right \"Activity\" (Design).", p.WorkitemId),
                        AssignedTo = p.AssignedTo,
                        WorkitemId = p.WorkitemId
                    });
                });
            }
        }
    }
}
