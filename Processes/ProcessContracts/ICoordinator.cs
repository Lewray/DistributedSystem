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
        /// <summary>
        /// Requests that a new task be allocated to an available process
        /// </summary>
        /// <param name="taskData">The task instructions to process</param>
        /// <returns>The ID of the newly created Task</returns>
        [OperationContract]
        int StartNewTask(string taskData);

        /// <summary>
        /// Requests a Progress update from the coordinator for a task
        /// identified by the taskId parameter
        /// </summary>
        /// <param name="taskId">The ID of the task to query</param>
        /// <returns>A TaskStatus object</returns>
        [OperationContract]
        TaskStatus CheckTaskProgress(int taskId);

        /// <summary>
        /// Requests a summary of the current system status in html format
        /// </summary>
        /// <returns>An HTML page in the form of a string</returns>
        [OperationContract]
        string GetCurrentSystemStatus();

        /// <summary>
        /// this is used to manually add a child process endpoint to a coordinator's internal list
        /// </summary>
        /// <param name="processServiceAddress">The URI of the child process service endpoint</param>
        [OperationContract]
        void Introduce(string processServiceAddress);

        /// <summary>
        /// Instructs the coordinator to perform a search for running child processes
        /// This is a local method only (not avaialable to other processes)
        /// </summary>
        void FindProcesses();

        /// <summary>
        /// Requests a copy of the coordinator's list of active child processes
        /// </summary>
        /// <returns>An IEnumerable interface to a collection of child process endpoint addresses</returns>
        IEnumerable<EndpointAddress> GetProcesses();
    }
}
