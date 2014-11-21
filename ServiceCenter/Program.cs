using ServiceCenter.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceCenter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Console.WriteLine("Please specify the fullname of the service to be executed");
                return;
            }
            IService service = Activator.CreateInstance(Type.GetType(args[0])) as IService;

            if (service == null)
            {
                Console.WriteLine("Invalid service name.");
                return;
            }
            service.Execute(service.ParseArgs(args));
        }
    }
}
