using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSUtilities
{
    public class TFSServiceContext : ServiceContext
    {
        public string ConnectionString { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
