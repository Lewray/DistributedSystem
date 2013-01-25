using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using ProcessContracts;

namespace Task
{
    class Program
    {
        static void Main(string[] args)
        {
            bool validArguments = true;

            int operand1, operand2;
            string op;
            string[] ops = new string[] { "+", "-", "*", "/" };

            if (args.Count() < 4)
            {
                Console.WriteLine("Required parameters are: operand 1, operand 2, operator, port number");

                validArguments = false;
            }
            else if (!int.TryParse(args[0], out operand1))
            {
                Console.WriteLine("Operand 1 must be of type Int32");

                validArguments = false;
            }
            else if (!int.TryParse(args[1], out operand2))
            {
                Console.WriteLine("Operand 2 must be of type Int32");

                validArguments = false;
            }
            else if (!ops.Contains(args[2]))
            {
                Console.WriteLine("Operator must be either '+', '-', '*' or '/'");

                validArguments = false;
            }

            if (validArguments)
            {
                TaskStatus t = StartTask(operand1, operand2, op, port);

                WaitForResult(t);
            }

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
        }

        private static void WaitForResult(TaskStatus t)
        {
            bool isComplete = false;

            do
            {
                Console.Write(".");
            
                Thread.Sleep(500);

                Console.Write(".");

                Thread.Sleep(500);

                Console.Write(".");

                Thread.Sleep(500);

                EndpointAddress a = new EndpointAddress(string.Format("tcp://localhost:{0}", port.ToString()));

                Binding binding = new NetTcpBinding();

                ITask proxy = ChannelFactory<ITask>.CreateChannel(binding, a);

                TaskStatus status = proxy.CheckProgress(t.TaskId);

                isComplete = status.Complete

                Console.WriteLine();
                Console.WriteLine("Status Report for Task ID:{0}", t.TaskId);
                Console.WriteLine("The task is currently {0}", status.Complete : "Complete" ? "Incomplete");
                Console.WriteLine("The current state is: {0}", status.ResultMessage);
                Console.WriteLine();
            }
            while(!isComplete);

            Console.WriteLine("Task Finished");
        }

        private static TaskStatus StartTask(int operand1, int operand2, string op, int port)
        {
            EndpointAddress a = new EndpointAddress(string.Format("tcp://localhost:{0}", port.ToString()));

            Binding binding = new NetTcpBinding();

            ITask proxy = ChannelFactory<ITask>.CreateChannel(binding, a);

            TaskStatus t = proxy.AcceptTask(operand1, operand2, op);

            return t;
        }
    }
}
