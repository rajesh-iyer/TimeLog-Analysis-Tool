using System;

namespace TimeLogManager
{
    [Serializable]
    public class TimeLogMilestone
    {        
        public DateTime CodeCompletionDate { get; set; }
        public DateTime CodeReviewCompletionDate { get; set; }
        public DateTime QACompletionDate { get; set; }
    }
}
