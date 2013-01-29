using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [ServiceContract(Name = "ChildProcess", Namespace = "http://ProcessContracts")]
    public interface IChildProcess : IProcess
    {
        // this is used to manually add a Coordinator endpoint to a child process internal list
        [OperationContract]
        void Introduce(string coordinatorServiceAddress);

        [OperationContract]
        void DoTask(Task task);

        [OperationContract]
        TaskStatus CheckTaskProgress(int taskId);

        void FindCoordinator();
    }
}
