using TimeLogManager;
using System.Linq;

namespace TimeLogManager.Analyzers
{
    public class TaskNamingConventionAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            var query = workitem.Tasks.Where(q => q.Title.StartsWith(workitem.WorkitemId.ToString()) == false);
            if (query.Any())
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Naming convention",
                    Title = "At least one task title does not start with PBI number.",
                });

                query.ToList().ForEach(p =>
                {
                    p.Observations.Add(new Observation
                    {
                        Code = "Naming convention",
                        Title = string.Format("Title of task #{0} does not start with PBI number.", p.WorkitemId),
                        AssignedTo = p.AssignedTo
                    });
                });
            }
        }
    }
}
