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

        private const string UnitTestingTaskTitle = "UNIT TESTING";
        private const string PeerCodeReviewTaskTitle = "PEER CODE REVIEW";
        private const string AnalysisTaskTitle = "IMPACT ANALYSIS";
        private const string ArchitectReviewTaskTitle = "ARCHITECT CODE REVIEW";
        private const string FunctionalTestingTaskTitle = "FUNCTIONAL TESTING";
        private const string TestcaseWritingTaskTitle = "TC WRITING";

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
<br /><br />
</div>";

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
                            Subject = string.Format("TimeMachine: {0} - TFS task status - {1}", developer, DateTime.Today.ToShortDateString()),
                            Body = string.Format(EmailTemplate, developer, DateTime.Today.ToLongDateString(), Context.SprintEndDate.ToLongDateString(), report)
                        });
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

        private string PrepareReport(string developer, List<WorkitemTimeDTO> list)
        {
            var developerList = list.Where(p => p.AssignedTo == developer).ToList();
            var xml = developerList.Serialize<List<WorkitemTimeDTO>>();
            return xml.TransformToTimeReporterFormat();
        }

        public List<WorkitemTimeDTO> GetTimeTrackingDetails(string iterationPath, DateTime estimatedEndDate)
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

        private void AnalyzeRelationMap(List<WorkitemTimeDTO> relationMap, DateTime estimatedEndDate)
        {
            var remainingCapacity = CalculateRemainingCapacity(estimatedEndDate);

            relationMap.ForEach(workitem =>
            {
                //mark all tasks as owned by developer and on further processing decide later
                workitem.Tasks.ForEach(task => task.Owner = TaskOwnerType.Developer);

                //All tasks should prefix with PBI Number
                workitem.AnyTaskTitleMissingPBINumber = workitem.Tasks.Where(q => q.Title.StartsWith(workitem.Id.ToString()) == false).Any();

                DoesUnitTestingTaskExist(UnitTestingTaskTitle, workitem);

                DoesImpactAnalysisTaskExist(AnalysisTaskTitle, workitem);
                DoesPeerReviewTaskExist(PeerCodeReviewTaskTitle, workitem);
                DoesArchitectCodeReviewTaskExist(ArchitectReviewTaskTitle, workitem);
                DoesFunctionalTestingTaskExist(FunctionalTestingTaskTitle, workitem);
                DoesTestcaseWritingTaskExist(TestcaseWritingTaskTitle, workitem);

                workitem.DevTaskActivityNotMatching = workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer && string.Compare(p.Activity, "Development", true) != 0).Any();
                workitem.QATaskActivityNotMatching = workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester && string.Compare(p.Activity, "Testing", true) != 0).Any();
                workitem.ReviewTaskActivityNotMatching = workitem.Tasks.Where(p => (p.Owner == TaskOwnerType.Peer || p.Owner == TaskOwnerType.Architect) && string.Compare(p.Activity, "Design", true) != 0).Any();

                //Check only developer owned tasks
                workitem.IsTaskMarkedAsDoneButNoTime = workitem.Tasks.Where(q => q.IsTaskMarkedAsDone == true && q.ActualDevEfforts == 0 && q.Owner == TaskOwnerType.Developer).Any();

                workitem.ActualDevEffortEntryGap = workitem.ActualDevEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer).Sum(q => q.ActualDevEfforts);
                workitem.PlannedDevEffortEntryGap = workitem.PlannedDevEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Developer).Sum(q => q.PlannedDevEfforts);

                //Note task level efforts for QA and Dev are all set in the DevEfforts fields
                workitem.ActualQAEffortEntryGap = workitem.ActualQAEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester).Sum(q => q.ActualDevEfforts);
                workitem.PlannedQAEffortEntryGap = workitem.PlannedQAEfforts - workitem.Tasks.Where(p => p.Owner == TaskOwnerType.Tester).Sum(q => q.PlannedDevEfforts);

                workitem.ExceedingTimeToComplete = (workitem.RemainingWork - remainingCapacity) > 0 ? workitem.RemainingWork - remainingCapacity : 0;

                workitem.HasUnassignedTasks = workitem.Tasks.Where(q => string.IsNullOrEmpty(q.AssignedTo) == true).Any();
            });
        }

        private static void DoesTestcaseWritingTaskExist(string TestcaseWritingTaskTitle, WorkitemTimeDTO workitem)
        {
            //get atleast one "TC Writing" task assigned to anyone
            var testingTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(TestcaseWritingTaskTitle) >= 0).ToList();
            if (testingTasks.Count > 0)
            {
                testingTasks.ForEach(p => p.Owner = TaskOwnerType.Tester); ;
                workitem.IsTCWritingTaskCreated = true;
            }
        }

        private static void DoesFunctionalTestingTaskExist(string TestingTaskTitle, WorkitemTimeDTO workitem)
        {
            //get atleast one "FUNCTIONAL TESTING" task assigned to anyone
            var testingTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(TestingTaskTitle) >= 0).ToList();
            if (testingTasks.Count > 0)
            {
                testingTasks.ForEach(p => p.Owner = TaskOwnerType.Tester);
                workitem.IsFunctionalTestingTaskCreated = true;
            }
        }

        private static void DoesArchitectCodeReviewTaskExist(string ArchitectReviewTaskTitle, WorkitemTimeDTO workitem)
        {
            //get atleast one "ARCHITECT CODE REVIEW" task assigned to anyone
            var architectCodeReviewTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(ArchitectReviewTaskTitle) >= 0).ToList();
            if (architectCodeReviewTasks.Count > 0)
            {
                architectCodeReviewTasks.ForEach(p => p.Owner = TaskOwnerType.Architect);
                workitem.IsArchitectReviewTaskCreated = true;
            }
        }

        private static void DoesImpactAnalysisTaskExist(string AnalysisTaskTitle, WorkitemTimeDTO workitem)
        {
            //get atleast one "IMPACT ANALYSIS" task assigned to anyone
            var analysisTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(AnalysisTaskTitle) >= 0).ToList();
            if (analysisTasks.Count > 0)
            {
                analysisTasks.ForEach(p => p.Owner = TaskOwnerType.Team);
                workitem.IsAnalysisTaskCreated = true;
            }
        }

        private static void DoesPeerReviewTaskExist(string PeerCodeReviewTaskTitle, WorkitemTimeDTO workitem)
        {
            //get atleast one "PEER CODE REVIEW" task assigned to anyone
            var peerCodeReviewTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(PeerCodeReviewTaskTitle) >= 0).ToList();
            if (peerCodeReviewTasks.Count > 0)
            {
                peerCodeReviewTasks.ForEach(p => p.Owner = TaskOwnerType.Peer);
                workitem.IsPeerReviewTaskCreated = true;
            }
        }

        private static void DoesUnitTestingTaskExist(string UnitTestingTaskTitle, WorkitemTimeDTO workitem)
        {
            //get atleast one "UNIT TESTING" task assigned to ME
            var unitTestTasks = workitem.Tasks.Where(q => q.Title.ToUpper().IndexOf(UnitTestingTaskTitle) >= 0 && string.Compare(workitem.AssignedTo, q.AssignedTo, true) == 0).ToList();
            if (unitTestTasks.Count > 0)
            {
                unitTestTasks.ForEach(p => p.Owner = TaskOwnerType.Developer);
                workitem.IsUnitTestTaskCreated = true;
            }
        }

        private int CalculateRemainingCapacity(DateTime estimatedEndDate)
        {
            var daysRemaining = estimatedEndDate.Date.Subtract(DateTime.Today.Date).Days;
            var daysToSubtract = Math.Floor((double)(daysRemaining / TotalDaysInWeek)) * (TotalDaysInWeek - WorkingDaysPerWeek);
            daysRemaining -= (int)daysToSubtract;
            var remainingCapacity = daysRemaining * CapacityPerDay;
            return remainingCapacity;
        }

        private List<WorkitemTimeDTO> BuildRelationMap(List<WorkitemTimeDTO> list, WorkItemLinkInfo[] links)
        {
            var pbiList = links.Where(p => p.SourceId == 0).Select(p => p.TargetId).ToList();
            var newList = list.Where(p => pbiList.Contains(p.Id)).ToList();
            foreach (var pbi in newList)
            {
                var taskList = links.Where(p => p.SourceId == pbi.Id).Select(p => p.TargetId);
                pbi.Tasks = list.Where(p => p.Type == "Task" && taskList.Contains(p.Id) && p.State != "Removed").ToList();
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
                                    AND Target.[System.State] <> 'Removed'
                                    AND Source.[System.IterationPath] Under @iterationPath
                                    AND [System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward'
                                    AND Target.[System.WorkItemType] <> ''
                            ORDER BY [Microsoft.VSTS.Common.StackRank], [Microsoft.VSTS.Common.Priority], [System.Id]";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("project", Context.Project);
            parameters.Add("iterationPath", iterationPath);

            return new Query(wiStore, queryText, parameters);
        }

        private List<WorkitemTimeDTO> ConvertToTimeTrackingDetails(WorkItemCollection results)
        {
            const string WorkItemTypeField = "Work Item Type";
            const string WorkItemPlannedDevEffortField = "Planned Effort(Dev)";
            const string WorkItemActualDevEffortField = "Actual Effort(Dev)";
            const string WorkItemPlannedQAEffortField = "Planned Effort(QA)";
            const string WorkItemActualQAEffortField = "Actual Effort(QA)";

            const string TaskPlannedEffortField = "Planned Effort";
            const string TaskActualEffortField = "Effort";

            const string RemainWorkField = "Remaining Work";
            const string AssignedToField = "Assigned To";
            const string StateField = "State";
            const string ActivityTypeField = "Activity";
            const string DevelopmentTrackingField = "Development Tracking";

            List<WorkitemTimeDTO> workItems = new List<WorkitemTimeDTO>();
            if (results != null)
            {
                foreach (WorkItem wi in results)
                {
                    workItems.Add(new WorkitemTimeDTO
                    {
                        Id = wi.Id,
                        Title = wi.Title,
                        Type = wi.Fields[WorkItemTypeField].Value.ToString(),
                        PlannedDevEfforts =
                                string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ?
                                    wi.Fields.Contains(TaskPlannedEffortField) && wi.Fields[TaskPlannedEffortField].Value != null
                                    ? float.Parse(wi.Fields[TaskPlannedEffortField].Value.ToString())
                                    : 0
                                :
                                    wi.Fields.Contains(WorkItemPlannedDevEffortField) && wi.Fields[WorkItemPlannedDevEffortField].Value != null
                                    ? float.Parse(wi.Fields[WorkItemPlannedDevEffortField].Value.ToString())
                                    : 0
                                ,
                        ActualDevEfforts =
                                string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ?
                                    wi.Fields.Contains(TaskActualEffortField) && wi.Fields[TaskActualEffortField].Value != null
                                    ? float.Parse(wi.Fields[TaskActualEffortField].Value.ToString())
                                    : 0
                                :
                                    wi.Fields.Contains(WorkItemActualDevEffortField) && wi.Fields[WorkItemActualDevEffortField].Value != null
                                    ? float.Parse(wi.Fields[WorkItemActualDevEffortField].Value.ToString())
                                    : 0,



                        PlannedQAEfforts =
                                string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ?
                                    wi.Fields.Contains(TaskPlannedEffortField) && wi.Fields[TaskPlannedEffortField].Value != null
                                    ? float.Parse(wi.Fields[TaskPlannedEffortField].Value.ToString())
                                    : 0
                                :
                                wi.Fields.Contains(WorkItemPlannedQAEffortField) && wi.Fields[WorkItemPlannedQAEffortField].Value != null
                                    ? float.Parse(wi.Fields[WorkItemPlannedQAEffortField].Value.ToString())
                                    : 0
                                ,
                        ActualQAEfforts =
                                string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ?
                                    wi.Fields.Contains(TaskActualEffortField) && wi.Fields[TaskActualEffortField].Value != null
                                    ? float.Parse(wi.Fields[TaskActualEffortField].Value.ToString())
                                    : 0
                                :
                                    wi.Fields.Contains(WorkItemActualQAEffortField) && wi.Fields[WorkItemActualQAEffortField].Value != null
                                    ? float.Parse(wi.Fields[WorkItemActualQAEffortField].Value.ToString())
                                    : 0,


                        RemainingWork = wi.Fields.Contains(RemainWorkField) && wi.Fields[RemainWorkField].Value != null
                                            ? float.Parse(wi.Fields[RemainWorkField].Value.ToString())
                                            : 0,
                        AssignedTo = wi.Fields[AssignedToField].Value.ToString(),
                        State = wi.Fields[StateField].Value.ToString(),

                        Activity = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ? wi.Fields[ActivityTypeField].Value.ToString()
                                : string.Empty,

                        DevelopmentTracking = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Product Backlog Item", true) == 0
                                ? wi.Fields[DevelopmentTrackingField].Value.ToString()
                                : string.Empty,

                        IsTaskMarkedAsDone = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                                && string.Compare(wi.Fields[StateField].Value.ToString(), "Done", true) == 0,

                        TrackingDate = DateTime.Today
                    });
                }
            }
            return workItems;
        }
    }
}
