using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Framework
{
    public abstract class BaseService : IService
    {
        private bool IsInitialized { get; set; }
        protected PluginManager PluginService { get; set; }
        public void Execute()
        {
            var context = OnInitialize();
            if (context != null)
            {
                if (PluginService == null)
                {
                    PluginService = new PluginManager();
                }

                PluginService.LoadPlugins(context.Plugins);
                IsInitialized = true;
                Run(context);
            }
        }

        protected abstract ServiceContext OnInitialize();
        protected abstract void Run(ServiceContext context);
    }
}
