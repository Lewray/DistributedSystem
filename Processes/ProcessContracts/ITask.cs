using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    /// <summary>
    /// This interface defines the methods which must be implemented in order for a process
    /// to accept tasks from a user through the command line.
    /// </summary>
    [ServiceContract(Name = "Task", Namespace = "http://ProcessContracts")]    
    public interface ITask
    {
        /// <summary>
        /// Accepts a new task from a user running the Task command from the commandline
        /// </summary>
        /// <param name="num1">The first operand</param>
        /// <param name="num2">The second operand</param>
        /// <param name="op">The operation to perform (+, -, *, /)</param>
        /// <returns>A Task Status object containing the Task's ID</returns>
        [OperationContract]
        TaskStatus AcceptTask(int num1, int num2, string op);

        /// <summary>
        /// Requests a progress update for a Task identified by the task ID provided
        /// </summary>
        /// <param name="taskId">The ID of the task to query</param>
        /// <returns>A TaskStatus object</returns>
        [OperationContract]
        TaskStatus CheckProgress(int taskId);
    }
}
