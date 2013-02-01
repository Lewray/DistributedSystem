using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [DataContract]
    public struct Task
    {
        /// <summary>
        /// The id of the task
        /// </summary>
        [DataMember]
        public int TaskId { get; set; }

        /// <summary>
        /// The address of the child process to which the task has been assigned
        /// </summary>
        [DataMember]
        public string AssignedProcessAddress { get; set; }

        /// <summary>
        /// The 'instructions' required to perform the task
        /// </summary>
        [DataMember]
        public string TaskData { get; set; }

        /// <summary>
        /// The current status of the task
        /// </summary>
        [DataMember]
        public TaskStatus Status { get; set; }

        /// <summary>
        /// The time at which the task was started
        /// </summary>
        [DataMember]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// The time at which the task was completed
        /// </summary>
        [DataMember]
        public DateTime EndTime { get; set; }
    }
}
