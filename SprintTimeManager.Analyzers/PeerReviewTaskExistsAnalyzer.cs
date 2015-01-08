using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager.Analyzers
{
    public class PeerReviewTaskExistsAnalyzer : TimeLogAnalyzer
    {
        public override void Analyze(TimeLogReportingServiceContext context, TimeLog workitem)
        {
            const string PeerCodeReviewTaskTitle = "PEER CODE REVIEW";
            //get atleast one "PEER CODE REVIEW" task assigned to anyone
            var peerCodeReviewTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(PeerCodeReviewTaskTitle) >= 0).ToList();
            if (peerCodeReviewTasks.Count > 0)
            {
                peerCodeReviewTasks.ForEach(p => p.Owner = TaskOwnerType.Peer);
            }
            else
            {
                workitem.Observations.Add(new Observation
                {
                    Code = "Lifecycle checklist",
                    Title = "Peer code review task not created yet. Please contact respective peer.",
                    AssignedTo = workitem.AssignedTo   //TODO: Logic to identify peer reviewer
                });
            }
        }
    }
}
