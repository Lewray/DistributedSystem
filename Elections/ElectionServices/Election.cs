using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DiscoveryUtils;
using ElectionContracts;

namespace ElectionServices
{
    public class Election : IElection
    {
        private static List<EndpointAddress> _electionServices = new List<EndpointAddress>();

        private static object _electionCollectionLock = new object();

        private static Dictionary<EndpointAddress, int> _failCount = new Dictionary<EndpointAddress,int>();

        private static bool _shutDown = false, _process = false, _coordinator = false;

        private static bool _electionInProcess = false, _startElection = false;

        private static DateTime _currentTimeStamp;

        private static TimeSpan _timeoutPeriod = new TimeSpan(0, 0, 30);

        private static object _timeStampLock = new object();

        private static EndpointAddress _thisAddress, _currentCoordinator;

        private Thread _worker;

        #region IElection Members

        public event EventHandler Elected;

        public int ConstituentCount
        {
            get 
            {
                int temp;

                lock (_electionCollectionLock)
                {
                    temp = _electionServices.Count;
                }

                return temp;
            }
        }

        public EndpointAddress ElectionIdentity
        {
            get
            {
                return _thisAddress;
            }
            set
            {
                _thisAddress = value;
            }
        }

        public void Introduce(string electionServiceAddress)
        {
            Console.WriteLine("Adding new election service address: {0}", electionServiceAddress);

            EndpointAddress a = new EndpointAddress(electionServiceAddress);

            lock (_electionCollectionLock)
            {
                if (!_electionServices.Contains(a))
                {
                    _electionServices.Add(a);
                }
            }
        }

        public void FindConstituents()
        {
            var addresses = DiscoveryHelper.DiscoverAddresses<IElection>();

            foreach (EndpointAddress address in addresses)
            {
                Console.Write("Found Election Service: ");
                Console.WriteLine(address);   
            }

            lock (_electionCollectionLock)
            {
                _electionServices.AddRange(addresses);
            }
        }

        public IEnumerable<EndpointAddress> GetConstituents()
        {
            List<EndpointAddress> temp;

            lock (_electionCollectionLock)
            {
                temp = new List<EndpointAddress>(_electionServices);
            }

            return temp;
        }       

        public void HeartbeatFromCoordinator(string endpointAddress)
        {
            // log the time
            lock (_timeStampLock)
            {
                _currentTimeStamp = DateTime.Now;
            }

            _currentCoordinator = new EndpointAddress(endpointAddress);
        }

        public bool Nomination(string address)
        {
            bool result = true;

            EndpointAddress a = new EndpointAddress(address);

            List<EndpointAddress> conflicts;

            lock (_electionCollectionLock)
            {
                // find any endpoint addresses which supersedes the address supplied as a parameter
                conflicts = _electionServices.FindAll(ea => (ea.IsGreaterThan(a) && !ea.Equals(_currentCoordinator)));
            }

            /*
             * foreach conflict found, attempt to elect each of them (in descending order)
             * remove any which cannot be contacted and respond to the Nominee with a false
             * if any conflicts respond successfully
             */
            if (conflicts != null && conflicts.Count > 0)
            {
                result = false;
            }
            else
            {
                _currentCoordinator = a;
                _currentTimeStamp = DateTime.Now;
            }

            return result;
        }

        public bool Elect()
        {
            bool temp = true;

            if (!_electionInProcess)
            {
                _startElection = true;
            }

            return temp;
        }

        public void SetCoordinator()
        {
            // start timed heartbeat messages

            _coordinator = true;
            _process = false;
            _shutDown = false;

            _worker = new Thread(new ThreadStart(DoCoordinatorWork));

            _worker.Start();
        }

        public void SetProcess()
        {
            _coordinator = false;
            _process = true;
            _shutDown = false;

            _worker = new Thread(new ThreadStart(DoProcessWork));

            _worker.Start();
        }

        public void Stop()
        {
            _shutDown = true;
            _coordinator = false;
            _process = false;
            _electionInProcess = false;
            _startElection = false;
        }

        #endregion

        private void DoCoordinatorWork()
        {
            while (!_shutDown && _coordinator)
            {
                Thread.Sleep(7000);

                if (!_shutDown && _coordinator)
                {
                    List<EndpointAddress> temp;

                    lock (_electionCollectionLock)
                    {
                        temp = new List<EndpointAddress>(_electionServices);
                    }

                    foreach (EndpointAddress a in temp)
                    {
                        Binding binding = new NetTcpBinding();

                        try
                        {
                            IElection proxy = ChannelFactory<IElection>.CreateChannel(binding, a);

                            proxy.HeartbeatFromCoordinator(ElectionIdentity.ToString());

                            // remove from fail list if the endpoint is contacted successfully
                            if (_failCount.ContainsKey(a))
                            {
                                _failCount.Remove(a);
                            }
                        }
                        catch (Exception ex)
                        {
                            // if contacting the endpoint failed then add to the fail list or increment
                            // the fail count if it already exists
                            if (!_failCount.ContainsKey(a))
                            {
                                _failCount.Add(a, 1);
                            }
                            else
                            {
                                _failCount[a] = (_failCount[a] + 1);
                            }
                        }
                    }

                    RemoveFailures();
                }
            }
        }

        private void RemoveFailures()
        {
 	        foreach (KeyValuePair<EndpointAddress, int> kvp in _failCount)
            {
                if (kvp.Value >= 3)
                {
                    lock (_electionCollectionLock)
                    {
                        _electionServices.Remove(kvp.Key);
                    }
                }
            }

            List<KeyValuePair<EndpointAddress, int>> addresses;

            lock (_electionCollectionLock)
            {
                addresses = new List<KeyValuePair<EndpointAddress,int>>(_failCount.Where(kvp => !_electionServices.Contains(kvp.Key)));
            }

            foreach (KeyValuePair<EndpointAddress, int> a in addresses)
            {
                _failCount.Remove(a.Key);
            }
        }

        private void DoProcessWork()
        {
            // prevent election from being started on start-up
            _currentTimeStamp = DateTime.Now;

            while (!_shutDown && _process)
            {
                Thread.Sleep(5);

                if (!_shutDown && _process)
                {
                    DateTime temp;

                    lock (_timeStampLock)
                    {
                        temp = _currentTimeStamp;
                    }

                    if (DateTime.Now > (temp.Add(_timeoutPeriod)))
                    {
                        _startElection = true;
                    }

                    if (_startElection)
                    {
                        StartElection();
                    }
                }
            }
        }

        private void StartElection()
        {
            _electionInProcess = true;
            _startElection = false;

            List<EndpointAddress> nominees;

            lock (_electionCollectionLock)
            {
                /*
                 * Determine if this service is next in line as the coordinator
                 */
                nominees = _electionServices.FindAll(ea => (ea.IsGreaterThan(ElectionIdentity) && !ea.Equals(_currentCoordinator)));
            }

            if (nominees.Count > 0)
            {
                //foreach(EndpointAddress a in nominees)
                //{
                //    Binding binding = new NetTcpBinding();

                //    try
                //    {
                //        IElection proxy = ChannelFactory<IElection>.CreateChannel(binding, a);

                //        proxy.Elect();

                //        break;
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine("Failed to Elect {0}", a.Uri.AbsoluteUri);
                //    }
                //}
            }
            else
            {
                bool approved = true;

                List<EndpointAddress> temp;

                lock (_electionCollectionLock)
                {
                    temp = new List<EndpointAddress>(_electionServices);
                }

                foreach (EndpointAddress a in nominees)
                {
                    Binding binding = new NetTcpBinding();

                    try
                    {
                        IElection proxy = ChannelFactory<IElection>.CreateChannel(binding, a);

                        if (!proxy.Nomination(ElectionIdentity.Uri.AbsoluteUri))
                        {
                            approved = false;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to send nomination to {0}", a.Uri.AbsoluteUri);
                        approved = false;
                    }
                }

                if (approved)
                {
                    Stop();

                    OnElected(new EventArgs());
                }
            }
        }

        private void OnElected(EventArgs e)
        {
            EventHandler handler = Elected;

            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
