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
        [OperationContract]
        TaskStatus AcceptTask(int num1, int num2, string op);

        [OperationContract]
        TaskStatus CheckProgress(int taskId);
    }
}
