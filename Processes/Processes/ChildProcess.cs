using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessContracts
{
    class ChildProcess : IChildProcess, ITask
    {
        private EndpointAddress _coordinatorAddress;

        private ServiceHost _processHost;

        private Queue<Task> _tasks = new Queue<Task>();

        private Dictionary<int, TaskStatus> _taskStatuses = new Dictionary<int, TaskStatus>();

        private Thread _worker;

        private bool _shutDown = false;

        #region IProcess Members

        public void Start()
        {
            Console.WriteLine("Starting local child process Service...");

            Uri baseAddress = DiscoveryUtils.DiscoveryHelper.AvailableTcpBaseAddress;

            this.FindCoordinator();

            _processHost = new ServiceHost(typeof(ChildProcess), baseAddress);

            _processHost.AddDefaultEndpoints();

            _processHost.Open();

            IntroduceToCoordinator();

            _worker.Start(new ThreadStart(DoWork));
        }

        public void Stop()
        {
            if (_processHost != null)
            {
                try
                {
                    _shutDown = true;
                    _processHost.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        #endregion

        #region ITask Members

        public TaskStatus AcceptTask(int num1, int num2, string op)
        {
            string taskData = num1.ToString() + "," + op + "," + num2.ToString();

            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, this._coordinatorAddress);

            int taskId = proxy.StartNewTask(taskData);

            TaskStatus ts = new TaskStatus();

            ts.TaskId = taskId;

            ts.Complete = false;
            ts.Successful = false;
            ts.ResultMessage = "Task started...";

            return ts;
        }

        public TaskStatus CheckProgress(int taskId)
        {
            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, this._coordinatorAddress);

            return proxy.CheckTaskProgress(taskId);
        }

        #endregion

        #region IChildProcess Members

        public void Introduce(string coordinatorServiceAddress)
        {
            _coordinatorAddress = new EndpointAddress(coordinatorServiceAddress);
        }

        public void DoTask(Task task)
        {
            _tasks.Enqueue(task);
            _taskStatuses.Add(task.TaskId, new TaskStatus() { TaskId = task.TaskId, Complete = false, Successful = false, ResultMessage = "Task Queued..." });
        }

        public void FindCoordinator()
        {
            var addresses = DiscoveryUtils.DiscoveryHelper.DiscoverAddresses<ICoordinator>();

            foreach (EndpointAddress address in addresses)
            {
                Console.WriteLine(address);
            }

            if (addresses.Count() <= 0)
            {
                throw new Exception("No Coordinator found");
            }

            _coordinatorAddress = addresses[0];
        }

        public TaskStatus CheckTaskProgress(int taskId)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void IntroduceToCoordinator()
        {
            Console.WriteLine("Introducing to: {0}", this._coordinatorAddress);

            Binding binding = new NetTcpBinding();

            ICoordinator proxy = ChannelFactory<ICoordinator>.CreateChannel(binding, this._coordinatorAddress);

            proxy.Introduce(this._processHost.Description.Endpoints[0].Address.ToString());            
        }

        private void DoWork()
        {
            while (!_shutDown)
            {
                Thread.Sleep(1000);

                if (_tasks.Count > 0)
                {
                    DoNextTask(_tasks.Dequeue());
                }
            }
        }

        private void DoNextTask(Task task)
        {
            string[] chunks = task.TaskData.Split(new[] { ',' });
        }        
    }
}
