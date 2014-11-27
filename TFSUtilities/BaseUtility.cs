using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities
{
    public class BaseUtility
    {
        protected TfsTeamProjectCollection projectCollection;
        protected TfsTeamProjectCollection Connect(TFSServiceContext context)
        {
            var token = new Microsoft.TeamFoundation.Client.SimpleWebTokenCredential(context.Username, context.Password);
            var clientCreds = new Microsoft.TeamFoundation.Client.TfsClientCredentials(token);
            projectCollection = new TfsTeamProjectCollection(new Uri(context.ConnectionString), clientCreds);
            projectCollection.EnsureAuthenticated();
            projectCollection.Connect(Microsoft.TeamFoundation.Framework.Common.ConnectOptions.None);
            return projectCollection;
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
