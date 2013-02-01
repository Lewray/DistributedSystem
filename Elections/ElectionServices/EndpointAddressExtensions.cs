using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ElectionServices
{
    public static class EndpointAddressExtensions
    {
        /// <summary>
        /// Determines whether an endpoint address should supersede another using a bespoke
        /// implementation of the IComparer interface.
        /// This is used to determine the next process to take on the role of coordinator
        /// </summary>
        /// <param name="x">The first service endpoint to compare</param>
        /// <param name="y">The second service endpoint to compare</param>
        /// <returns>True if x is 'greater than' y</returns>
        public static bool IsGreaterThan(this EndpointAddress x, EndpointAddress y)
        {
            IComparer<EndpointAddress> comparer = new NominationEndointComparer();

            return (comparer.Compare(x, y)) > 0;
        }
    }
}
