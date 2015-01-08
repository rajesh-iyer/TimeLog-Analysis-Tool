using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager
{
    [Serializable]
    public class TimeLogData
    {
        public TimeLogReportingServiceContext ServiceContext { get; set; }
        public List<TimeLog> Workitems { get; set; }
        public TeamMemberProfile TeamMember { get; set; }
    }
}
