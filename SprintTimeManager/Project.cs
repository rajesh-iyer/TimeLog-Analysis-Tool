﻿using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLogManager
{
    public class Project : IDTitle
    {
        public virtual Organization Organization { get; set; }
    }
}
