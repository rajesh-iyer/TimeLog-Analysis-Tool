using ServiceCenter.Framework;
using System.Collections.Generic;

namespace TimeLogManager
{
    public abstract class TimeLogDataProvider : Plugin
    {
        public abstract List<TimeLog> LoadData(TimeLogReportingServiceContext context);

        public TimeLogDataProvider()
        {
            PluginType = typeof(TimeLogDataProvider).FullName;
        }
    }
}
