using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities
{
    [Serializable]
    public class WorkitemTime
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AssignedTo { get; set; }
        public double PlannedEfforts { get; set; }
        public double ActualEfforts { get; set; }
        public double RemainingWork { get; set; }
        public string Type { get; set; }

        public DateTime TrackingDate { get; set; }
        public List<WorkitemTime> Tasks { get; set; }

        //Process columns at workitem level
        public bool AreAllImpedimentsClosed { get; set; }
        public bool IsStoryMarkedReadyForBuildButTaskIncomplete { get; set; }

        //Process columns at task level
        public bool IsTaskMarkedAsDone { get; set; }
        public bool IsTaskMarkedAsDoneButNoTime { get; set; }

        public string State { get; set; }
        
        public bool IsUnitTestTaskCreated { get; set; }
        public bool IsUnitTestTaskMarkedAsDone { get; set; }

        public bool IsAnalysisTaskCreated { get; set; }
        public bool IsAnalysisTaskMarkedAsDone { get; set; }        

        public bool IsPeerReviewTaskCreated { get; set; }
        public bool IsPeerReviewTaskMarkedAsDone { get; set; }

        public bool IsArchitectReviewTaskCreated { get; set; }
        public bool IsArchitectReviewTaskMarkedAsDone { get; set; }

        public bool IsTestingTaskCreated { get; set; }
        public bool IsTestingTaskMarkedAsDone { get; set; }

        //properties for analysis
        public double ActualEffortEntryGap { get; set; }
        public double PlannedEffortEntryGap { get; set; }
        public double RemainingEffortEntryGap { get; set; }
        public double ExceedingTimeToComplete { get; set; }
        
        public bool HasUnassignedTasks { get; set; }
    }
}
