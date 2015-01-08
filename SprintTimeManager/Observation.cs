using ServiceCenter.Framework;
using System;
namespace TimeLogManager
{
    public enum ObservationType
    {
        Workitem,
        Task,
        Generic
    }
    public class Observation : IDTitle
    {
        public Project Project { get; set; }
        public string Code { get; set; }
        public int Points { get; set; }
        public DateTime Timestamp { get; set; }
        
        public int WorkitemId { get; set; }
        
        public ObservationType Type { get; set; }
        public string AssignedTo { get; set; }

        public Observation()
        {
            Timestamp = DateTime.Now.Date;
            Type = ObservationType.Workitem;
        }
    }
}
