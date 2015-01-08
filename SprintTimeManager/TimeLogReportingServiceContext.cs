using ServiceCenter.Framework;
using ServiceCenter.Framework.Notification;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TimeLogManager
{
    [Serializable]
    public class TimeLogReportingServiceContext : ServiceContext
    {
        public Organization Organization { get; set; }
        public Project Project { get; set; }
        
        public string IterationPath { get; set; }

        public List<Milestone> Milestones { get; set; }

        public DateTime? GetMilestone(string milestoneTitle)
        {
            if (Milestones == null)
            {
                return null;
            }
            return Milestones.Where(p => string.Compare(p.Title, milestoneTitle, true) == 0).Select(p => p.CompletionDate).SingleOrDefault();
        }
    }
}
