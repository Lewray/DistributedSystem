using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [ServiceContract(Name = "Task", Namespace = "http://ProcessContracts")]
    public interface ITask
    {
        [OperationContract]
        TaskStatus AcceptTask(int num1, int num2, string op);

        [OperationContract]
        TaskStatus CheckProgress(int taskId);
    }
}
