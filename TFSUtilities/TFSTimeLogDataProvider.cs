using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using ServiceCenter.Framework;
using TimeLogManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities
{
    public class TFSTimeLogDataProvider : TimeLogDataProvider
    {
        public string ConnectionString { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SkipPBIWithTitles { get; set; }

        private const string CodeCompletionDate = "CodeCompletionDate";
        private const string CodeReviewCompletionDate = "CodeReviewCompletionDate";
        private const string QACompletionDate = "QACompletionDate";

        public override List<TimeLog> LoadData(TimeLogReportingServiceContext context)
        {
            string iterationPath = context.IterationPath;
            //var estimatedEndDate = context.GetMilestone(CodeCompletionDate);

            var wiStore = Connect(context).GetService<WorkItemStore>();

            //get linked work items
            //http://blogs.msdn.com/b/jsocha/archive/2012/02/22/retrieving-tfs-results-from-a-tree-query.aspx
            Query treeQuery = PrepareTreeQuery(wiStore, iterationPath, context.Project.Title);
            WorkItemLinkInfo[] links = treeQuery.RunLinkQuery();
            WorkItemCollection linkedResults = GetAssociatedWorkItems(wiStore, treeQuery, links);
            var linkedList = ConvertToTimeTrackingDetails(linkedResults);
            var relationMap = BuildRelationMap(linkedList, links);

            //get unlinked workitems
            WorkItemCollection results = GetAllWorkItems(wiStore, iterationPath, context.Project.Title);
            var list = ConvertToTimeTrackingDetails(results);
            var idList = relationMap.Select(q => q.WorkitemId).ToList();
            relationMap.AddRange(list.Where(p => !idList.Contains(p.WorkitemId)));

            SkipSpecificTitlePBIs(relationMap);
            return relationMap;
        }

        private WorkItemCollection GetAllWorkItems(WorkItemStore wiStore, string iterationPath, string project)
        {
            var queryText = @"SELECT [System.Id], 
                                    [System.Title], 
                                    [Microsoft.VSTS.Common.BacklogPriority], 
                                    [System.AssignedTo], 
                                    [System.State], 
                                    [Microsoft.VSTS.Scheduling.RemainingWork], 
                                    [Microsoft.VSTS.CMMI.Blocked], 
                                    [System.WorkItemType] 
                                FROM WorkItems 
                                WHERE [System.TeamProject] = @project 
                                    AND [System.WorkItemType] IN ('Product Backlog Item', 'Bug')
                                    AND [System.State] <> 'Removed'         
                                    AND [System.IterationPath] Under @iterationPath
                                    ORDER BY [Microsoft.VSTS.Common.Priority], [System.Id] ";

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("project", project);
            parameters.Add("iterationPath", iterationPath);

            var query = new Query(wiStore, queryText, parameters);

            return query.RunQuery();
        }

        private void SkipSpecificTitlePBIs(List<TimeLog> relationMap)
        {
            if (string.IsNullOrEmpty(SkipPBIWithTitles) == false && relationMap != null)
            {
                SkipPBIWithTitles.Split(',').ToList().ForEach(title =>
                {
                    relationMap.RemoveAll(p => p.Title.ToLower().StartsWith(title.ToLower()));
                });
            }
        }

        private TfsTeamProjectCollection projectCollection;
        private TfsTeamProjectCollection Connect(ServiceContext context)
        {
            var token = new Microsoft.TeamFoundation.Client.SimpleWebTokenCredential(Username, Password);
            var clientCreds = new Microsoft.TeamFoundation.Client.TfsClientCredentials(token);
            projectCollection = new TfsTeamProjectCollection(new Uri(ConnectionString), clientCreds);
            projectCollection.EnsureAuthenticated();
            projectCollection.Connect(Microsoft.TeamFoundation.Framework.Common.ConnectOptions.None);
            return projectCollection;
        }

        private List<TimeLog> BuildRelationMap(List<TimeLog> list, WorkItemLinkInfo[] links)
        {
            var pbiList = links.Where(p => p.SourceId == 0).Select(p => p.TargetId).ToList();
            var newList = list.Where(p => pbiList.Contains(p.WorkitemId)).ToList();
            foreach (var pbi in newList)
            {
                var taskList = links.Where(p => p.SourceId == pbi.WorkitemId).Select(p => p.TargetId);
                pbi.Tasks = list.Where(p => p.Type == "Task" && taskList.Contains(p.WorkitemId) && p.State != "Removed").ToList();
            }

            return newList;
        }

        private static WorkItemCollection GetAssociatedWorkItems(WorkItemStore wiStore, Query treeQuery, WorkItemLinkInfo[] links)
        {
            int[] ids = links.Select(p => p.TargetId).Distinct().ToArray();

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

        private Query PrepareTreeQuery(WorkItemStore wiStore, string iterationPath, string project)
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
                                    AND Source.[System.WorkItemType] IN ('Product Backlog Item', 'Bug')
                                    AND Source.[System.State] <> 'Removed'         
                                    AND Source.[System.IterationPath] Under @iterationPath
                                    ORDER BY [Microsoft.VSTS.Common.Priority], [System.Id] ";

            //AND ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward' || [System.Links.LinkType] = 'System.LinkTypes.Related')

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("project", project);
            parameters.Add("iterationPath", iterationPath);

            return new Query(wiStore, queryText, parameters);
        }

        private List<TimeLog> ConvertToTimeTrackingDetails(WorkItemCollection results)
        {
            const string WorkItemTypeField = "Work Item Type";
            const string WorkItemPlannedDevEffortField = "Planned Effort(Dev)";
            const string WorkItemActualDevEffortField = "Actual Effort(Dev)";
            const string WorkItemPlannedQAEffortField = "Planned Effort(QA)";
            const string WorkItemActualQAEffortField = "Actual Effort(QA)";
            const string IterationPathField = "Iteration Path";

            const string TaskPlannedEffortField = "Planned Effort";
            const string TaskActualEffortField = "Effort";

            const string RemainWorkField = "Remaining Work";
            const string AssignedToField = "Assigned To";
            const string StateField = "State";
            const string ActivityTypeField = "Activity";
            const string DevelopmentTrackingField = "Development Tracking";

            List<TimeLog> workItems = new List<TimeLog>();
            if (results != null)
            {
                foreach (WorkItem wi in results)
                {
                    //foreach(Field f in wi.Fields){
                    //    Debug.WriteLine("{0}: {1}", f.Name, f.Value);
                    //}
                    var workitem = new TimeLog
                    {
                        WorkitemId = wi.Id,
                        Title = wi.Title,
                        Type = wi.Fields[WorkItemTypeField].Value.ToString(),
                        IterationPath = wi.Fields[IterationPathField].Value.ToString(),
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


                        //RemainingWork = wi.Fields.Contains(RemainWorkField) && wi.Fields[RemainWorkField].Value != null
                        //                    ? float.Parse(wi.Fields[RemainWorkField].Value.ToString())
                        //                    : wi.Fields[RemainWorkField].Value,

                        AssignedTo = wi.Fields[AssignedToField].Value.ToString(),
                        State = wi.Fields[StateField].Value.ToString(),

                        Activity = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                ? wi.Fields[ActivityTypeField].Value.ToString()
                                : string.Empty,

                        DevelopmentTracking = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Product Backlog Item", true) == 0
                                                || string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Bug", true) == 0
                                ? wi.Fields[DevelopmentTrackingField].Value.ToString()
                                : string.Empty,

                        IsTaskMarkedAsDone = string.Compare(wi.Fields[WorkItemTypeField].Value.ToString(), "Task", true) == 0
                                                && string.Compare(wi.Fields[StateField].Value.ToString(), "Done", true) == 0,

                        TrackingDate = DateTime.Today.Date
                    };

                    if (wi.Fields.Contains(RemainWorkField) && wi.Fields[RemainWorkField].Value != null)
                    {
                        workitem.RemainingWork = float.Parse(wi.Fields[RemainWorkField].Value.ToString());
                    }

                    workItems.Add(workitem);
                }
            }

            return workItems;
        }

    }
}
