using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MSDN_Threading
{
    public class Worker
    {
        private volatile bool _shouldStop;

        public void doSomeThing()
        {
            while (!_shouldStop)
            {
                Console.WriteLine("doing something...");
            }
            Console.WriteLine("terminated gracefully...");
        }

        public void requestStop()
        {
            _shouldStop = true;
        }
    }

    class Program
    {
        static AutoResetEvent autoEvent;

        static void DoWork()
        {
            Console.WriteLine("workers thread started, no waiting on event...");
            autoEvent.WaitOne();
            Console.WriteLine("reactived, now exiting...");
        }

        static void Main(string[] args)
        {
            //lec 1
            autoEvent = new AutoResetEvent(false);

            Console.WriteLine("main starting worker thread...");
            Thread t = new Thread(DoWork);
            t.Start();

            Console.WriteLine("main sleep 1 second...");
            Thread.Sleep(1000);

            Console.WriteLine("main signaling worker thread...");
            autoEvent.Set();


            //lec2
            Worker worker = new Worker();
            Thread workerThread = new Thread(worker.doSomeThing);
            workerThread.Start();
        }
    }
}
