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
        public static bool IsGreaterThan(this EndpointAddress x, EndpointAddress y)
        {
            IComparer<EndpointAddress> comparer = new NominationEndointComparer();

            return (comparer.Compare(x, y)) > 0;
        }
    }
}
