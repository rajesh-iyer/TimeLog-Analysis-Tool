using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class PBIMilestoneAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            if (string.Compare(workitem.Type, ProductBacklogItem, true) == 0)
            {
                var codeCompletionDate = context.GetMilestone("Code Completion Date");
                var reviewCompletionDate = context.GetMilestone("Code Review Completion Date");
                var qaCompletionDate = context.GetMilestone("QA Completion Date");

                ValidateCodeCompleteMilestone(workitem, codeCompletionDate);
                ValidateCodeReviewMilestone(workitem, reviewCompletionDate);
                ValidateQAMilestone(workitem, qaCompletionDate);
            }
        }

        private static void ValidateCodeCompleteMilestone(TimeLog workitem, DateTime? codeCompletionDate)
        {
            if (codeCompletionDate.HasValue && codeCompletionDate.Value < DateTime.Now)
            {
                if (string.Compare(workitem.DevelopmentTracking, "Ready for Build", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Ready for Feature Testing", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Feature Testing Complete", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Pending Code Review", true) != 0)
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Milestone",
                        Title = "Code completion date not achieved"
                    });
                }
            }
        }

        private static void ValidateCodeReviewMilestone(TimeLog workitem, DateTime? reviewCompletionDate)
        {
            if (reviewCompletionDate.HasValue && reviewCompletionDate.Value < DateTime.Now)
            {
                if (string.Compare(workitem.DevelopmentTracking, "Ready for Build", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Ready for Feature Testing", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Feature Testing Complete", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Pending Code Review", true) != 0)
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Milestone",
                        Title = "Code review request date not achieved"
                    });
                }
            }
        }

        private static void ValidateQAMilestone(TimeLog workitem, DateTime? qaCompletionDate)
        {
            if (qaCompletionDate.HasValue && qaCompletionDate.Value < DateTime.Now)
            {
                if (string.Compare(workitem.DevelopmentTracking, "Feature Testing Complete", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Ready for Feature Testing", true) != 0
                    && string.Compare(workitem.DevelopmentTracking, "Feature Testing Complete", true) != 0)
                {
                    workitem.Observations.Add(new Observation
                    {
                        Code = "Milestone",
                        Title = "QA testing date not achieved"
                    });
                }
            }
        }
    }
}
