using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities.DTO
{
    public enum TaskOwnerType
    {
        Developer,
        Peer,
        Architect,
        Tester,
        Team
    }

    [Serializable]
    public class WorkitemTimeDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AssignedTo { get; set; }

        public double PlannedDevEfforts { get; set; }
        public double ActualDevEfforts { get; set; }

        public double PlannedQAEfforts { get; set; }
        public double ActualQAEfforts { get; set; }

        public double RemainingWork { get; set; }
        public string Type { get; set; }
        public string DevelopmentTracking { get; set; }
        public string Activity { get; set; }

        public DateTime TrackingDate { get; set; }
        public List<WorkitemTimeDTO> Tasks { get; set; }

        public TaskOwnerType Owner { get; set; }

        //Process columns at workitem level
        public bool AreAllImpedimentsClosed { get; set; }
        public bool IsStoryMarkedReadyForBuildButTaskIncomplete { get; set; }

        //Process columns at task level
        public bool IsTaskMarkedAsDone { get; set; }
        public bool IsTaskMarkedAsDoneButNoTime { get; set; }

        public string State { get; set; }
        
        public bool IsUnitTestTaskCreated { get; set; }
        public bool IsAnalysisTaskCreated { get; set; }
        public bool IsPeerReviewTaskCreated { get; set; }
        public bool IsArchitectReviewTaskCreated { get; set; }
        public bool IsFunctionalTestingTaskCreated { get; set; }
        public bool IsTCWritingTaskCreated { get; set; }

        //public bool IsUnitTestTaskMarkedAsDone { get; set; }
        //public bool IsAnalysisTaskMarkedAsDone { get; set; }
        //public bool IsPeerReviewTaskMarkedAsDone { get; set; }
        //public bool IsArchitectReviewTaskMarkedAsDone { get; set; }
        //public bool IsFunctionalTestingTaskMarkedAsDone { get; set; }
        //public bool IsTCWritingTaskMarkedAsDone { get; set; }        

        public bool AnyTaskTitleMissingPBINumber { get; set; }

        public bool DevTaskActivityNotMatching { get; set; }
        public bool QATaskActivityNotMatching { get; set; }
        public bool ReviewTaskActivityNotMatching { get; set; }

        //properties for analysis
        public double ActualDevEffortEntryGap { get; set; }
        public double PlannedDevEffortEntryGap { get; set; }
        public double ActualQAEffortEntryGap { get; set; }
        public double PlannedQAEffortEntryGap { get; set; }

        public double ExceedingTimeToComplete { get; set; }
        
        public bool HasUnassignedTasks { get; set; }
    }
}
