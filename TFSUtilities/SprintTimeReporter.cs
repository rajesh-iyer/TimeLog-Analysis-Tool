using Microsoft.TeamFoundation.WorkItemTracking.Client;
using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TFSUtilities.DTO;

namespace TFSUtilities
{
    public class SprintTimeReporter : BaseUtility, IService
    {        
        private const int CapacityPerDay = 7;
        private const int TotalDaysInWeek = 7;
        private const int WorkingDaysPerWeek = 5;

        public SprintTimeReporterServiceContext Context { get; set; }

        public ServiceContext ParseArgs(string[] args)
        {
            var context = new SprintTimeReporterServiceContext();

            //first parameter is the name of the dll and dll parameters are passed thereafter
            for (int i = 1; i < args.Length; i++)
            {
                var param = args[i].Split(':');
                if (param.Length >= 2)
                {
                    if (string.Compare(param[0], "-cn", true) == 0)
                        context.ConnectionString = ExtractParameter(param);
                    if (string.Compare(param[0], "-usr", true) == 0)
                        context.Username = ExtractParameter(param);
                    if (string.Compare(param[0], "-pwd", true) == 0)
                        context.Password = ExtractParameter(param);
                    if (string.Compare(param[0], "-itr", true) == 0)
                        context.IterationPath = ExtractParameter(param);
                    if (string.Compare(param[0], "-pj", true) == 0)
                        context.Project = ExtractParameter(param);
                    if (string.Compare(param[0], "-ed", true) == 0)
                    {
                        DateTime date;
                        if (DateTime.TryParse(ExtractParameter(param), out date))
                        {
                            context.SprintEndDate = date;
                        }
                        else
                        {
                            context.SprintEndDate = DateTime.MaxValue;
                        }
                    }
                }
            }
            return context;
        }

        public void Execute(ServiceContext context)
        {
            const string EmailTemplate =
@"<div style='font-family:Arial, sans-serif;font-size:12px;'>{0}, 
<br /><br />
You task status for {1}.
<br /><br />
<b>The sprint end date is {2}</b>
<br /><br />{3}. 

<br /><br /> - Project Management Team</div>";

            Context = context as SprintTimeReporterServiceContext;
            if (Context != null)
            {
                List<DevProfileDTO> devEmailMap = InitializeDevProfileMap();
                var list = GetTimeTrackingDetails(Context.IterationPath, Context.SprintEndDate);
                var developers = list.Select(p => p.AssignedTo).Distinct().ToList();

                NotificationManager notifier = new NotificationManager();
                var managerEmailList = devEmailMap.Where(p => p.Role == "Manager").Select(p => p.Email).ToList();
                var ccEmails = string.Empty;
                if (managerEmailList != null && managerEmailList.Count > 0)
                {
                    ccEmails = string.Join(",", managerEmailList);
                }
                foreach (var developer in developers)
                {
                    string report = PrepareReport(developer, list);
                    string devEmail = devEmailMap.Where(p => p.Fullname == developer).Select(p => p.Email).SingleOrDefault();
                    if (string.IsNullOrEmpty(devEmail) == false)
                    {
                        notifier.SendEmail(new EmailContext
                        {
                            To = devEmail,
                            CC = ccEmails,
                            Subject = string.Format("TimeMachine: {0} your TFS task status for {1}", developer, DateTime.Today.ToShortDateString()),
                            Body = string.Format(EmailTemplate, developer, DateTime.Today.ToLongDateString(), Context.SprintEndDate.ToLongDateString(), report)
                        });

                        Thread.Sleep(1000);
                    }
                }
            }
        }

        private List<DevProfileDTO> InitializeDevProfileMap()
        {            
            string filename = "DevProfiles.xml";
            var list = filename.DeserializeFile<List<DevProfileDTO>>();
            return list;
        }

        private string PrepareReport(string developer, List<WorkitemTime> list)
        {
            var developerList = list.Where(p => p.AssignedTo == developer).ToList();
            var xml = developerList.Serialize<List<WorkitemTime>>();
            return xml.TransformToTimeReporterFormat();
        }

        private string ExtractParameter(string[] param)
        {
            string value = param[1];
            for (int i = 2; i < param.Length; i++)
            {
                value += ":" + param[i];
            }
            return value;
        }

        public List<WorkitemTime> GetTimeTrackingDetails(string iterationPath, DateTime estimatedEndDate)
        {
            var wiStore = Connect(Context).GetService<WorkItemStore>();

            //http://blogs.msdn.com/b/jsocha/archive/2012/02/22/retrieving-tfs-results-from-a-tree-query.aspx

            Query treeQuery = PrepareTreeQuery(iterationPath, wiStore);
            WorkItemLinkInfo[] links = treeQuery.RunLinkQuery();
            WorkItemCollection results = GetAssociatedWorkItems(wiStore, treeQuery, links);

            var list = ConvertToTimeTrackingDetails(results);
            var relationMap = BuildRelationMap(list, links);
            AnalyzeRelationMap(relationMap, estimatedEndDate);
            return relationMap;
        }

        private void AnalyzeRelationMap(List<WorkitemTime> relationMap, DateTime estimatedEndDate)
        {
            const string UnitTestingTaskTitle = "UNIT TESTING";
            const string PeerCodeReviewTaskTitle = "PEER CODE REVIEW";
            const string AnalysisTaskTitle = "ANALYSIS";
            const string ArchitectReviewTaskTitle = "ARCHITECT CODE REVIEW";
            const string TestingTaskTitle = "TESTING";

            var remainingCapacity = CalculateRemainingCapacity(estimatedEndDate);

            relationMap.ForEach(p =>
            {
                var unitTestTask = p.Tasks.Where(q => q.Title.ToUpper().IndexOf(UnitTestingTaskTitle) > 0).FirstOrDefault();
                if (unitTestTask != null)
                {
                    p.IsUnitTestTaskCreated = true;
                    if (string.Compare(unitTestTask.State, "Done", true) == 0)
                    {
                        p.IsUnitTestTaskMarkedAsDone = true;
                    } 
                }
                var peerCodeReviewTask = p.Tasks.Where(q => q.Title.ToUpper().IndexOf(PeerCodeReviewTaskTitle) > 0).FirstOrDefault();
                if (peerCodeReviewTask != null)
                {
                    p.IsPeerReviewTaskCreated = true;
                    if (string.Compare(peerCodeReviewTask.State, "Done", true) == 0)
                    {
                        p.IsPeerReviewTaskMarkedAsDone = true;
                    }
                }
                var analysisTask = p.Tasks.Where(q => q.Title.ToUpper().IndexOf(AnalysisTaskTitle) > 0).FirstOrDefault();
                if (analysisTask != null)
                {
                    p.IsAnalysisTaskCreated = true;
                    if (string.Compare(analysisTask.State, "Done", true) == 0)
                    {
                        p.IsAnalysisTaskMarkedAsDone = true;
                    }
                }
                var architectCodeReviewTask = p.Tasks.Where(q => q.Title.ToUpper().IndexOf(ArchitectReviewTaskTitle) > 0).FirstOrDefault();
                if (architectCodeReviewTask != null)
                {
                    p.IsArchitectReviewTaskCreated = true;
                    if (string.Compare(architectCodeReviewTask.State, "Done", true) == 0)
                    {
                        p.IsArchitectReviewTaskMarkedAsDone = true;
                    }
                }
                var testingTask = p.Tasks.Where(q => q.Title.ToUpper().IndexOf(TestingTaskTitle) > 0).FirstOrDefault();
                if (testingTask != null)
                {
                    p.IsTestingTaskCreated = true;
                    if (string.Compare(testingTask.State, "Done", true) == 0)
                    {
                        p.IsTestingTaskMarkedAsDone = true;
                    }
                }

                p.IsTaskMarkedAsDoneButNoTime = p.Tasks.Where(q => q.IsTaskMarkedAsDone == true && q.ActualEfforts == 0).Any();

                p.ActualEffortEntryGap = p.ActualEfforts - p.Tasks.Sum(q => q.ActualEfforts);
                p.PlannedEffortEntryGap = p.PlannedEfforts - p.Tasks.Sum(q => q.PlannedEfforts);
                p.RemainingEffortEntryGap = p.RemainingWork - p.Tasks.Sum(q => q.RemainingWork);
                p.ExceedingTimeToComplete = p.RemainingWork - remainingCapacity;

                p.HasUnassignedTasks = p.Tasks.Where(q => string.IsNullOrEmpty(q.AssignedTo) == true).Any();
            });
        }

        private int CalculateRemainingCapacity(DateTime estimatedEndDate)
        {
            var daysRemaining = estimatedEndDate.Date.Subtract(DateTime.Today.Date).Days;
            var daysToSubtract = Math.Floor((double)(daysRemaining / TotalDaysInWeek)) * (TotalDaysInWeek - WorkingDaysPerWeek);
            daysRemaining -= (int)daysToSubtract;

            var remainingCapacity = daysRemaining * CapacityPerDay;
            return remainingCapacity;
        }

        private List<WorkitemTime> BuildRelationMap(List<WorkitemTime> list, WorkItemLinkInfo[] links)
        {
            var pbiList = links.Where(p => p.SourceId == 0).Select(p => p.TargetId).ToList();
            var newList = list.Where(p => pbiList.Contains(p.Id)).ToList();
            foreach (var pbi in newList)
            {
                var taskList = links.Where(p => p.SourceId == pbi.Id).Select(p => p.TargetId);
                pbi.Tasks = list.Where(p => p.Type == "Task" && taskList.Contains(p.Id) &&
                                        p.State != "Removed" &&
                                        (string.Compare(p.AssignedTo, pbi.AssignedTo, true) == 0 || string.IsNullOrEmpty(p.AssignedTo) == true)).ToList();
            }

            return newList;
        }

        private static WorkItemCollection GetAssociatedWorkItems(WorkItemStore wiStore, Query treeQuery, WorkItemLinkInfo[] links)
        {
            int[] ids = links.Select(p => p.TargetId).ToArray();

            var detailsWiql = new StringBuilder();
            detailsWiql.AppendLine("SELECT");
            bool first = true;
            foreach (FieldDefinition field in treeQuery.DisplayFieldList)
            {
                detailsWiql.Append("    ");
                if (!first)
                    detailsWiql.Append(",");
                detailsWiql.AppendLine("[" + field.ReferenceName + "]");
                first = false;
            }
            detailsWiql.AppendLine("FROM WorkItems");

            var flatQuery = new Query(wiStore, detailsWiql.ToString(), ids);
            WorkItemCollection results = flatQuery.RunQuery();
            return results;
        }

        private Query PrepareTreeQuery(string iterationPath, WorkItemStore wiStore)
        {

            var queryText = @"SELECT [System.Id], 
                                    [System.Title], 
                                    [Microsoft.VSTS.Common.BacklogPriority], 
                                    [System.AssignedTo], 
                                    [System.State], 
                                    [Microsoft.VSTS.Scheduling.RemainingWork], 
                                    [Microsoft.VSTS.CMMI.Blocked], 
                                    [System.WorkItemType] 
                                FROM WorkItemLinks 
                                WHERE Source.[System.TeamProject] = @project 
                                    AND Source.[System.WorkItemType] IN ('Product Backlog Item', 'Task')
                                    AND Source.[System.State] <> 'Removed'         
                                    AND Source.[System.IterationPath] Under @iterationPath
                                    AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward'
                                    AND Target.[System.WorkItemType] <> ''
                            ORDER BY [Microsoft.VSTS.Common.StackRank], [Microsoft.VSTS.Common.Priority], [System.Id]";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("project", Context.Project);
            parameters.Add("iterationPath", iterationPath);

            return new Query(wiStore, queryText, parameters);
        }

        private List<WorkitemTime> ConvertToTimeTrackingDetails(WorkItemCollection results)
        {
            const string WorkItemTypeField = "Work Item Type";
            const string WorkItemPlannedEffortField = "Planned Effort(Dev)";
            const string WorkItemActualEffortField = "Actual Effort(Dev)";

            const string TaskPlannedEffortField = "Planned Effort";
            const string TaskActualEffortField = "Actual Effort";

            const string RemainWorkField = "Remaining Work";
            const string AssignedToField = "Assigned To";
            const string StateField = "State";

            List<WorkitemTime> workItems = new List<WorkitemTime>();
            if (results != null)
            {
                foreach (WorkItem wi in results)
                {
                    workItems.Add(new WorkitemTime
                    {
                        Id = wi.Id,
                        Title = wi.Title,
                        Type = wi.Fields[WorkItemTypeField].Value.ToString(),
                        PlannedEfforts =
                                string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ?
                                    wi.Fields.Contains(TaskPlannedEffortField) && wi.Fields[TaskPlannedEffortField].Value != null
                                    ? float.Parse(wi.Fields[TaskPlannedEffortField].Value.ToString())
                                    : 0
                                :
                                    wi.Fields.Contains(WorkItemPlannedEffortField) && wi.Fields[WorkItemPlannedEffortField].Value != null
                                    ? float.Parse(wi.Fields[WorkItemPlannedEffortField].Value.ToString())
                                    : 0
                                ,

                        ActualEfforts =
                                string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ?
                                    wi.Fields.Contains(TaskActualEffortField) && wi.Fields[TaskActualEffortField].Value != null
                                    ? float.Parse(wi.Fields[TaskActualEffortField].Value.ToString())
                                    : 0
                                :                        
                                    wi.Fields.Contains(WorkItemActualEffortField) && wi.Fields[WorkItemActualEffortField].Value != null
                                    ? float.Parse(wi.Fields[WorkItemActualEffortField].Value.ToString())
                                    : 0,

                        RemainingWork = wi.Fields.Contains(RemainWorkField) && wi.Fields[RemainWorkField].Value != null
                                            ? float.Parse(wi.Fields[RemainWorkField].Value.ToString())
                                            : 0,
                        AssignedTo = wi.Fields[AssignedToField].Value.ToString(),
                        State = wi.Fields[StateField].Value.ToString(),
                        IsTaskMarkedAsDone = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0 && string.Compare(wi.Fields[StateField].Value.ToString(), "Done", true) == 0,

                        TrackingDate = DateTime.Today
                    });
                }
            }
            return workItems;
        }



    }
}
