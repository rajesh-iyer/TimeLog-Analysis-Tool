using ServiceCenter.Framework;
using ServiceCenter.Framework.Notification;
using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TimeLogManager
{
    public class TimeLogReportingService : BaseService
    {
        private const int CapacityPerDay = 7;
        private const int TotalDaysInWeek = 7;
        private const int WorkingDaysPerWeek = 5;

        public TimeLogReportingServiceContext Context { get; set; }

        protected override ServiceContext OnInitialize()
        {
            return ParseArgs(Environment.GetCommandLineArgs());
        }

        protected override void Run(ServiceContext context)
        {
            Context = context as TimeLogReportingServiceContext;
            if (Context == null)
            {
                throw new Exception("Configuration parameters not found");
            }

            var notifiers = PluginService.GetPlugins<Notifier>(typeof(Notifier).FullName);
            var formatter = PluginService.GetPlugin<TimeLogDataFormatter>(typeof(TimeLogDataFormatter).FullName);
            var dataProvider = PluginService.GetPlugin<TimeLogDataProvider>(typeof(TimeLogDataProvider).FullName);
            var analyzers = PluginService.GetPlugins<TimeLogAnalyzer>(typeof(TimeLogAnalyzer).FullName);

            if (analyzers != null && analyzers.Count > 0)
            {
                analyzers.Sort();
            }
            var workitems = dataProvider.LoadData(Context);

            AnalyzeWorkitems(Context, workitems, analyzers);

            var developers = workitems.Select(p => p.AssignedTo).Distinct().ToList();

            foreach (var developer in developers)
            {
                TimeLogData data = new TimeLogData
                {
                    ServiceContext = Context,
                    TeamMember = Context.TeamProfiles.Where(p => p.Fullname == developer).SingleOrDefault(),
                    Workitems = workitems.Where(p => p.AssignedTo == developer).ToList()
                };
                string devEmail = Context.TeamProfiles.Where(p => p.Fullname == developer).Select(p => p.Email).SingleOrDefault();
                if (string.IsNullOrEmpty(devEmail) == false)
                {
                    notifiers.ForEach(p =>
                        {
                            try
                            {
                                p.Notify(new TimeLogNotificationContext { Data = data, DataFormatter = formatter });
                            }
                            catch (Exception ex)
                            {
                                //Log exception
                                Console.WriteLine("Error occured: {0}", ex.Message);
                            }
                        });
                }
            }
        }

        private void AnalyzeWorkitems(TimeLogReportingServiceContext context, List<TimeLog> list, List<TimeLogAnalyzer> analyzers)
        {
            if (list != null && list.Count > 0 && analyzers != null && analyzers.Count > 0)
            {
                list.ForEach(workitem =>
                {
                    analyzers.ForEach(p => p.Analyze(context, workitem));
                    //attach workitem to each observation
                    workitem.Observations.ForEach(observation =>
                    {
                        observation.WorkitemId = workitem.WorkitemId;
                        observation.Type = ObservationType.Workitem;
                    });

                    //attach workitem to each tasklevel observation
                    workitem.Tasks.ForEach(task => task.Observations.ForEach(observation =>
                                                    {
                                                        observation.WorkitemId = workitem.WorkitemId;
                                                        observation.Type = ObservationType.Task;
                                                    })
                                        );
                });
            }
        }

        private ServiceContext ParseArgs(string[] args)
        {
            //first parameter is the name of the dll and dll parameters are passed thereafter
            for (int i = 1; i < args.Length; i++)
            {
                var param = args[i].Split(':');
                if (param.Length >= 2)
                {
                    if (string.Compare(param[0], "-manifest", true) == 0)
                    {
                        var filename = ExtractParameter(param);
                        return filename.DeserializeFile<TimeLogReportingServiceContext>();
                    }
                }
            }
            return null;
        }

        protected string ExtractParameter(string[] param)
        {
            string value = param[1];
            for (int i = 2; i < param.Length; i++)
            {
                value += ":" + param[i];
            }
            return value;
        }
    }
}
