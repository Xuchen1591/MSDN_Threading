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

    public class Producer
    {
        private Queue<int> _queue;

        public Producer(Queue<int> q)
        {
            _queue = q;
        }

        public void ThreadRun()
        {
            int count = 0;
            Random r = new Random();
            lock((ICollection<int>)_queue)
            {
                while(_queue.Count < 20)
                {
                    _queue.Enqueue(r.Next(0,100));
                    count++;
                }
            }
            Console.WriteLine("producer thread produce {0} items", count);
        }
    }

    public class Consumer
    {
        private Queue<int> _queue;

        public Consumer(Queue<int> q)
        {
            _queue = q;
        }

        public void ThreadRun()
        {
            int count = 0;
            lock((ICollection<int>)_queue)
            {
                int item = _queue.Dequeue();
                count ++;
            }
            Console.WriteLine("consumer thread consume {0} items",count);
        }
    }

    //lecture 3: threading pool
    public class Fibonacci
    {
        private int _n;
        public int N { get { return _n; } }

        public int FibOfN { get { return _fibOfN; } }
        private int _fibOfN;

        private ManualResetEvent _doneEvent;
        public Fibonacci(int n,ManualResetEvent doneEvent)
        {
            _n = n;
            _doneEvent = doneEvent;
        }

        public void ThreadPoolCallback(Object threadContext)
        {
            int threadIndex = (int)threadContext;
            Console.WriteLine("thread{0} started...", threadIndex);
            _fibOfN = Calculate(_n);
            Console.WriteLine("thread{0} calculated...", threadIndex);
            _doneEvent.Set();
        }

        public int Calculate(int n)
        {
            if (n <= 1)
            {
                return n;
            }
            return Calculate(n - 1) + Calculate(n - 2);
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

        private static void ShowQueueContents(Queue<int> q)
        {
            lock((ICollection<int>)q)
            {
                foreach(int i in q)
                    Console.Write(i + ",");
                Console.WriteLine();
                Console.WriteLine();
            }
        }

        static void Main(string[] args)
        {
            //lec 1
            /*
            autoEvent = new AutoResetEvent(false);

            Console.WriteLine("main starting worker thread...");
            Thread t = new Thread(DoWork);
            t.Start();

            Console.WriteLine("main sleep 1 second...");
            Thread.Sleep(1000);

            Console.WriteLine("main signaling worker thread...");
            autoEvent.Set();
             */


            /*
            //lec2
            Worker worker = new Worker();
            Thread workerThread = new Thread(worker.doSomeThing);
            workerThread.Start();

            while (!workerThread.IsAlive) ;

            Thread.Sleep(1);

            worker.requestStop();
            workerThread.Join();
            Console.WriteLine("main: work threading terminated...");
             * */

            /*
            Queue<int> queue = new Queue<int>();

            Console.WriteLine("configuring worker threads...");
            Producer producer = new Producer(queue);
            Consumer consumer = new Consumer(queue);
            Thread producerThread = new Thread(producer.ThreadRun);
            Thread consumerThread = new Thread(consumer.ThreadRun);

            Console.WriteLine("launching producer and consumer threads...");
            producerThread.Start();
            consumerThread.Start();

            for(int i=0;i<4;i++)
            {
                Thread.Sleep(2500);
                ShowQueueContents(queue);
            }

            Console.WriteLine("signaling the threading to termibating...");

            producerThread.Join();
            consumerThread.Join();
             * */


            //lecture 3: threading pool
            const int FibonacciCalculations = 10;

            // One event is used for each Fibonacci object
            ManualResetEvent[] doneEvents = new ManualResetEvent[FibonacciCalculations];
            Fibonacci[] fibArray = new Fibonacci[FibonacciCalculations];
            Random r = new Random();

            // Configure and launch threads using ThreadPool:
            Console.WriteLine("launching {0} tasks...", FibonacciCalculations);
            for (int i = 0; i < FibonacciCalculations; i++)
            {
                doneEvents[i] = new ManualResetEvent(false);
                Fibonacci f = new Fibonacci(r.Next(20, 40), doneEvents[i]);
                fibArray[i] = f;
                ThreadPool.QueueUserWorkItem(f.ThreadPoolCallback, i);
            }

            // Wait for all threads in pool to calculation...
            WaitHandle.WaitAll(doneEvents);
            Console.WriteLine("All calculations are complete.");

            // Display the results...
            for (int i = 0; i < FibonacciCalculations; i++)
            {
                Fibonacci f = fibArray[i];
                Console.WriteLine("Fibonacci({0}) = {1}", f.N, f.FibOfN);
            }
        }
    }
}
