using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ElectionServices
{
    /// <summary>
    /// Compares two endpoint addresses using the host (if it is an IP address) and port number (if provided)
    /// If the hosts are defined as IP addresses then the octets are compared one by one starting with the most significant.
    /// If a single octet is greater then that endpoint address is considered the greater value.
    /// 
    /// If both IP addresses are identical or they cannot be compared because one or both of the hosts is a string then the port
    /// numbers specified are compared. The higher value of port being considered the greater value of endpoint address.
    /// 
    /// If neither of these methods can be performed then an argument exception is thrown.
    /// </summary>
    public class NominationEndointComparer : IComparer<EndpointAddress>
    {
        #region IComparer<EndpointAddress> Members

        public int Compare(EndpointAddress x, EndpointAddress y)
        {
            /*
             * x is less than y return less than zero
             * x is equal to y return 0
             * x is greater than y return greater than zero
            */

            bool compared = false;
            IPAddress xIp, yIp;
            int result = 0;

            if (IPAddress.TryParse(x.Uri.Host, out xIp))
            {
                byte[] xBytes = xIp.GetAddressBytes();
                
                if (IPAddress.TryParse(y.Uri.Host, out yIp))
                {
                    byte[] yBytes = yIp.GetAddressBytes();

                    for (int i = 0; i < xBytes.Length; i++)
                    {
                        result = xBytes[i].CompareTo(yBytes[i]);

                        if (result != 0)
                        {
                            compared = true;
                            break;
                        }
                    }
                }
            }

            if (!compared)
            {
                int xPort, yPort;

                try
                {
                    xPort = x.Uri.Port;
                    yPort = y.Uri.Port;
                    
                    result = xPort.CompareTo(yPort);

                    compared = true;                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to get ports from uri");
                }                
            }

            if (!compared)
            {
                throw new ArgumentException("Could not compare the EndpointAddresses provided"); 
            }

            return result;
        }

        #endregion
    }
}
