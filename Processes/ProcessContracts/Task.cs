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
        [DataMember]
        public int TaskId { get; set; }

        [DataMember]
        public string AssignedProcessAddress { get; set; }

        [DataMember]
        public string TaskData { get; set; }

        [DataMember]
        public TaskStatus Status { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }
    }
}
