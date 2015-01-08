using ServiceCenter.Framework;
using System;
using System.Collections.Generic;

namespace TimeLogManager
{
    public class DefaultHtmlTimeLogDataFormatter : TimeLogDataFormatter
    {
        public string TransformXslFile { get; set; }

        public override string FormatTitle(TimeLogData data)
        {
            return string.Format("TimeMachine: {0} - {1}", data.TeamMember.Fullname, data.ServiceContext.IterationPath);
        }

        public override string FormatData(TimeLogData data)
        {
            var xml = data.Serialize<TimeLogData>();
            return xml.TransformToTimeReporterFormat(TransformXslFile);
        }
    }
}
