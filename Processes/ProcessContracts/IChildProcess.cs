using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [ServiceContract(Name = "Process", Namespace = "http://ProcessContracts")]
    public interface IChildProcess : IProcess
    {
        // this is used to manually add an Coordinator endpoint to a processes internal list
        [OperationContract]
        void Introduce(string coordinatorServiceAddress);

        [OperationContract]
        void DoTask(Task task);

        void FindCoordinator();
    }
}
