using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCenter.Framework
{
    public class ServiceContext
    {
        public List<TeamMemberProfile> TeamProfiles { get; set; }
        public List<Plugin> Plugins { get; set; }
    }
}
