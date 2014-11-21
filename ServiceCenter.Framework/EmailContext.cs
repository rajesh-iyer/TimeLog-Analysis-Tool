using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServiceCenter.Framework
{
    public class EmailContext   
    {
        public string To { get; set; }
        public string From { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
