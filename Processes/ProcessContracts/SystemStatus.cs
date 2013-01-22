using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ProcessContracts
{
    [DataContract]
    public struct SystemStatus
    {
        [DataMember]
        List<string> ProcessAddresses { get; set; }

        [DataMember]
        string CoordinatorAddress { get; set; }

        [DataMember]
        List<string> CurrentTasks { get; set; }
    }
}
