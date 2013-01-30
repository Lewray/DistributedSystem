using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

using ProcessContracts;

namespace Processes
{
    class Coordinator : ICoordinator, ITask
    {
        private static Dictionary<int, Task> _tasks = new Dictionary<int, Task>();

        private static Queue<EndpointAddress> _processes = new Queue<EndpointAddress>();

        ServiceHost _coordinatorHost;

        private static int _nextTaskId = 0;

        private static int NextTaskId
        {
            get
            {
                int temp = _nextTaskId++;

                return temp;
            }
        }

        #region IProcess Members

        public void Start()
        {
            this.StartCoordinator();
        }

        public void Stop()
        {
            if (_coordinatorHost != null)
            {
                try
                {
                    _coordinatorHost.Close();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }                
            }
        }

        #endregion

        #region ICoordinatorService Members

        public int StartNewTask(string taskData)
        {
            Task t = new Task() { TaskId = NextTaskId, TaskData = taskData, Status = new TaskStatus() };

            // get next available process
            EndpointAddress address = _processes.Dequeue();

            // set Task assigned process address
            t.AssignedProcessAddress = address.ToString();

            Console.WriteLine("Task ID {0} Created", t.TaskId);
            Console.WriteLine("Task assigned to {0}", t.AssignedProcessAddress);
            Console.WriteLine();

            _tasks.Add(t.TaskId, t);

            // assign task to process
            Binding binding = new NetTcpBinding();

            IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, address);

            _processes.Enqueue(address);

            proxy.DoTask(t);

            return t.TaskId;
        }

        public TaskStatus CheckTaskProgress(int taskId)
        {
            Task t = _tasks[taskId];

            Binding binding = new NetTcpBinding();

            IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, new EndpointAddress(t.AssignedProcessAddress));

            return proxy.CheckTaskProgress(taskId);
        }

        public SystemStatus GetCurrentSystemStatus()
        {
            throw new NotImplementedException();
        }

        public void Introduce(string processServiceAddress)
        {
            Console.WriteLine("adding new process service address: {0}", processServiceAddress);

            EndpointAddress a = new EndpointAddress(processServiceAddress);

            if (!_processes.Contains(a))
            {
                _processes.Enqueue(a);
            }   
        }

        public int ProcessCount
        {
            get { throw new NotImplementedException(); }
        }

        public void FindProcesses()
        {
            var addresses = DiscoveryUtils.DiscoveryHelper.DiscoverAddresses<IChildProcess>();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine(address);
            }

            _processes = new Queue<EndpointAddress>(addresses);
        }

        public IEnumerable<System.ServiceModel.EndpointAddress> GetProcesses()
        {
            return _processes;
        }

        #endregion

        #region ITask Members

        public TaskStatus AcceptTask(int num1, int num2, string op)
        {
            string taskData = num1.ToString() + "," + op + "," + num2.ToString();

            int taskId = this.StartNewTask(taskData);

            TaskStatus ts = new TaskStatus();

            ts.TaskId = taskId;

            ts.Complete = false;
            ts.Successful = false;
            ts.ResultMessage = "Task started...";

            return ts;
        }

        public TaskStatus CheckProgress(int taskId)
        {
            return this.CheckTaskProgress(taskId);
        }

        #endregion

        private void StartCoordinator()
        {
            Console.WriteLine("Starting local Coordinator Service...");

            Uri baseAddress = DiscoveryUtils.DiscoveryHelper.AvailableTcpBaseAddress;

            this.FindProcesses();

            _coordinatorHost = new ServiceHost(typeof(Coordinator), baseAddress);

            _coordinatorHost.AddDefaultEndpoints();

            _coordinatorHost.Open();

            IntroduceToProcesses();
        }

        private void IntroduceToProcesses()
        {
            var processes = this.GetProcesses();

            foreach (EndpointAddress address in _processes)
            {
                Console.WriteLine("Introducing to: {0}", address);

                Binding binding = new NetTcpBinding();

                IChildProcess proxy = ChannelFactory<IChildProcess>.CreateChannel(binding, address);

                proxy.Introduce(this._coordinatorHost.Description.Endpoints[1].Address.ToString());
            }
        }
    }
}
