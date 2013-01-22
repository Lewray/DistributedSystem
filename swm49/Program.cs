using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processes;
using ProcessContracts;

namespace swm49
{
    class Program
    {
        static void Main(string[] args)
        {
            IProcess p = new Process();

            p.Start();

            Console.ReadLine();

            p.Stop();
        }
    }
}
