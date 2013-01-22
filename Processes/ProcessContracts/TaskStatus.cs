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
        [DataMember]
        public int TaskId { get; set; }

        [DataMember]
        public bool Complete { get; set; }

        [DataMember]
        public bool Successful { get; set; }

        [DataMember]
        public string ResultMessage { get; set; }
    }
}
