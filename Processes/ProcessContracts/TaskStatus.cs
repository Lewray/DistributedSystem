using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [DataContract]
    public struct TaskStatus
    {
        /// <summary>
        /// The auto generated id of the task
        /// </summary>
        [DataMember]
        public int TaskId { get; set; }

        /// <summary>
        /// boolean indication of whether or not the task has been completed
        /// </summary>
        [DataMember]
        public bool Complete { get; set; }

        /// <summary>
        /// boolean indication of whether or not the task was completed successfully
        /// </summary>
        [DataMember]
        public bool Successful { get; set; }

        /// <summary>
        /// A message giving the result of the task
        /// </summary>
        [DataMember]
        public string ResultMessage { get; set; }
    }
}
