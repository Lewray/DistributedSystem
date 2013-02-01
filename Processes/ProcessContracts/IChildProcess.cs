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
        /// <summary>
        /// This is used to manually add a Coordinator endpoint to a child process' internal list
        /// </summary>
        /// <param name="coordinatorServiceAddress">URI of the service endpoint address</param>
        [OperationContract]
        void Introduce(string coordinatorServiceAddress);

        /// <summary>
        /// Assigns a task to the child process
        /// </summary>
        /// <param name="task"></param>
        [OperationContract]
        void DoTask(Task task);

        /// <summary>
        /// Requests details of the progress on a task identified by the task id
        /// </summary>
        /// <param name="taskId">ID of the task to query</param>
        /// <returns>A TaskStatus object</returns>
        [OperationContract]
        TaskStatus CheckTaskProgress(int taskId);

        /// <summary>
        /// Tells the child process to search for a running coordinator
        /// This is a local method only (not avaialable to other processes)
        /// </summary>
        void FindCoordinator();
    }
}
