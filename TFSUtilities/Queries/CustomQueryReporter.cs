using Microsoft.TeamFoundation.WorkItemTracking.Client;
using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFSUtilities.DTO;

namespace TFSUtilities
{
    public class CustomQueryReporter : IService
    {
        public CustomQueryReporterServiceContext Context { get; set; }

        public ServiceContext ParseArgs(string[] args)
        {
            var context = new CustomQueryReporterServiceContext();

            //first parameter is the name of the dll and dll parameters are passed thereafter
            for (int i = 1; i < args.Length; i++)
            {
                var param = args[i].Split(':');
                //if (param.Length >= 2)
                //{
                //    if (string.Compare(param[0], "-cn", true) == 0)
                //        context.ConnectionString = ExtractParameter(param);
                //    if (string.Compare(param[0], "-usr", true) == 0)
                //        context.Username = ExtractParameter(param);
                //    if (string.Compare(param[0], "-pwd", true) == 0)
                //        context.Password = ExtractParameter(param);
                //    if (string.Compare(param[0], "-itr", true) == 0)
                //        context.IterationPath = ExtractParameter(param);
                //    if (string.Compare(param[0], "-pj", true) == 0)
                //        context.Project = ExtractParameter(param);
                //    if (string.Compare(param[0], "-cmd", true) == 0)
                //        context.CommandName = ExtractParameter(param);
                //}
            }
            return context;
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

        public void Execute(ServiceContext context)
        {
            Context = context as CustomQueryReporterServiceContext;
            if (Context != null)
            {
                var customQueries = InitializeCustomQueryMap();
                var queryToExecute = customQueries.Where(p => string.Compare(p.CommandName, Context.CommandName, true) == 0).SingleOrDefault();
                if (queryToExecute == null)
                {
                    Console.WriteLine("Query command - {0} does not match.", Context.CommandName);
                    return;
                }

                string query = string.Format(queryToExecute.Query, Context.IterationPath);
                var list = ExecuteQuery(query);
            }
        }

        private object ExecuteQuery(string query)
        {
            //var wiStore = Connect(Context).GetService<WorkItemStore>();
            //var flatQuery = new Query(wiStore, query, null);
            //WorkItemCollection results = flatQuery.RunQuery();
            //return results;
            throw new NotImplementedException();
        }

        private List<CustomQueryDTO> InitializeCustomQueryMap()
        {
            string filename = "CustomTFSQueries.xml";
            return filename.DeserializeFile<List<CustomQueryDTO>>();
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
