using ServiceCenter.Framework;
using ServiceCenter.Framework.Notification;
using TimeLogManager;
using System.Collections.Generic;

namespace TimeLogManager
{
    public interface ITimeLogService : IService
    {
        List<Notifier> LoadNotifiers();
        List<TimeLogAnalyzer> LoadAnalyzers();
        List<TimeLog> LoadWorkItems();
    }
}
