using ServiceCenter.Framework;
using System;
using System.Collections.Generic;

namespace TimeLogManager
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
    public class TimeLog : IDTitle
    {
        public virtual Project Project { get; set; }

        public string InternalId { get; set; }  //generally used for external system for its own purpose e.g. mongodb objectid
        public int WorkitemId { get; set; }
        public string AssignedTo { get; set; }
        public string IterationPath { get; set; }

        public double PlannedDevEfforts { get; set; }
        public double ActualDevEfforts { get; set; }

        public double PlannedQAEfforts { get; set; }
        public double ActualQAEfforts { get; set; }

        public double? RemainingWork { get; set; }
        public string Type { get; set; }
        public string DevelopmentTracking { get; set; }
        public string Activity { get; set; }

        public List<TimeLog> Tasks { get; set; }

        public TaskOwnerType Owner { get; set; }

        //Process columns at task level
        public bool IsTaskMarkedAsDone { get; set; }

        public string State { get; set; }
        
        public List<Observation> Observations { get; set; }

        public DateTime TrackingDate { get; set; }
        public TimeLog()
        {
            Observations = new List<Observation>();
            Tasks = new List<TimeLog>();
            TrackingDate = DateTime.Now;
        }
    }
}
