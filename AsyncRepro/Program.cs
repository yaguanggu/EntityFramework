using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;

namespace AsyncRepro
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Main");
            var reset = new ManualResetEventSlim(false);

            Console.WriteLine("Calling Start");
            var spinTask = Task.Factory.StartNew(async () => { await Spin(reset); });

            Console.WriteLine("Start Waiting on reset event");
            reset.Wait();

            Console.WriteLine("Waiting on task");
            spinTask.Wait();

            Console.Write("Done");
            Console.ReadLine();
        }

        public static async Task Spin(ManualResetEventSlim resetEvent)
        {
            for (var index = 0; index < int.MaxValue; index++)
            {
                try
                {
                    Console.WriteLine("Constructing {0}", index);
                    var testClass = new AsyncQuerySqlServerTest(new NorthwindQuerySqlServerFixture());

                    Console.WriteLine("Await Starting");
                    await testClass.Contains_with_subquery();

                    Console.WriteLine("Await Done");
                }
                catch (Exception e)
                {
                    Debugger.Launch();
                }
            }

            resetEvent.Set();
        }
    }
}
