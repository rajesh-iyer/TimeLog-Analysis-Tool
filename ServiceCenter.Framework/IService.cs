using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter.Framework
{
    public interface IService
    {
        ServiceContext ParseArgs(string[] args);
        void Execute(ServiceContext context);
    }
}
