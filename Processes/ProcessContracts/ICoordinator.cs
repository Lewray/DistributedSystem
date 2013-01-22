using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace ProcessContracts
{
    [ServiceContract(Name = "Coordinator", Namespace = "http://ProcessContracts")]
    public interface ICoordinator : IProcess
    {
        [OperationContract]
        int StartNewTask(string taskData);

        [OperationContract]
        TaskStatus CheckTaskProgress(int taskId);

        [OperationContract]
        SystemStatus GetCurrentSystemStatus();

        // this is used to manually add a process endpoint to a processes internal list
        [OperationContract]
        void Introduce(string processServiceAddress);

        int ProcessCount { get; }

        void FindProcesses();

        IEnumerable<EndpointAddress> GetProcesses();
    }
}
