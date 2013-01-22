using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

using ElectionContracts;
using ElectionServices;

namespace ElectionFactories
{
    public static class ElectionFactory
    {
        private static Election _election;

        public static IElection CreateElection()
        {
            _election = new Election();

            return _election;
        }

        public static IElection GetElection()
        {
            if (_election == null)
            {
                throw new ArgumentNullException("_election");  
            }

            return _election;
        }
    }
}
