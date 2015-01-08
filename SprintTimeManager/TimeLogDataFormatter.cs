using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager
{
    public abstract class TimeLogDataFormatter : DataFormatter
    {
        public abstract string FormatTitle(TimeLogData data);
        public abstract string FormatData(TimeLogData data);

        public TimeLogDataFormatter()
        {
            PluginType = typeof(TimeLogDataFormatter).FullName;
        }
    }
}
