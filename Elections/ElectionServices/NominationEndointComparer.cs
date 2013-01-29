using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace ElectionServices
{
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
