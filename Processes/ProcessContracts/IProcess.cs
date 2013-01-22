using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [ServiceContract(Name = "Process", Namespace = "http://ProcessContracts")]
    public interface IProcess
    {
        void Start();

        void Stop();
    }
}
