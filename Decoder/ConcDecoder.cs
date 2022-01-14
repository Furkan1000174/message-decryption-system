using System;
using Decoder;
using System.Threading;
using System.Collections.Generic;

namespace ConcDecoder
{
    /// <summary>
    /// A concurrent version of the class Buffer
    /// Note: For the final solution this class MUST be implemented.
    /// </summary>
    public class ConcurrentTaskBuffer : TaskBuffer
    {
        //todo: add required fields such that satisfies a thread safe shared buffer.
        private Mutex mutex;
        private Semaphore provider_sem, worker_sem;
        
        public ConcurrentTaskBuffer() : base()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            this.mutex = new Mutex();
            this.provider_sem = new Semaphore(1,1);
            this.worker_sem = new Semaphore(0,1);
        }

        /// <summary>
        /// Adds the given task to the queue. The implementation must support concurrent accesses.
        /// </summary>
        /// <param name="task">A task to wait in the queue for the execution</param>
        public override void AddTask(TaskDecryption task)
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            lock(mutex){
            this.taskBuffer.Enqueue(task);
            this.numOfTasks++;
            this.maxBuffSize = this.taskBuffer.Count > this.maxBuffSize ? this.taskBuffer.Count  : this.maxBuffSize;
            this.LogVisualisation();
            this.PrintBufferSize();
            }
        }

        /// <summary>
        /// Picks the next task to be executed. The implementation must support concurrent accesses.
        /// </summary>
        /// <returns>Next task from the list to be executed. Null if there is no task.</returns>
        public override TaskDecryption GetNextTask()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            TaskDecryption t = null;
            lock(mutex){
            if (this.taskBuffer.Count > 0)
            {
                t = this.taskBuffer.Dequeue();
                // check if the task is the last ending task: put the task back.
                // It is an indication to terminate processors
                if (t.id < 0)
                    this.taskBuffer.Enqueue(t);
            }
            //this.worker_sem.Release();
            }
            return t;
        }

        /// <summary>
        /// Prints the number of elements available in the buffer.
        /// </summary>
        public override void PrintBufferSize()
        {
            //todo: implement this method such that satisfies a thread safe shared buffer.
            lock(mutex){
            Console.Write("Buffer#{0} ; ", this.taskBuffer.Count);
            }
        }
    }

    class ConcLaunch : Launch
    {
        public ConcLaunch() : base(){  }

        /// <summary>
        /// This method implements the concurrent version of the decryption of provided challenges.
        /// </summary>
        /// <param name="numOfProviders">Number of providers</param>
        /// <param name="numOfWorkers">Number of workers</param>
        /// <returns>Information logged during the execution.</returns>
        public string ConcurrentTaskExecution(int numOfProviders, int numOfWorkers)
        {
            ConcurrentTaskBuffer tasks = new ConcurrentTaskBuffer();

            //todo: implement this method such that satisfies a thread safe shared buffer.
            Provider[] providers = new Provider[numOfProviders];
            Worker[] workers = new Worker[numOfWorkers];
            LinkedList<Thread> threads = new LinkedList<Thread>();

            for (int i = 0; i < numOfProviders; i++)
                providers[i] = new Provider(tasks, this.challenges);

            for (int i = 0; i < numOfWorkers; i++)
                workers[i] = new Worker(tasks);

            for (int i = 0; i < numOfProviders; i++)
            {
                //threads.AddFirst(new Thread(providers[i].SendTasks));
                Thread provider = new Thread(providers[i].SendTasks);
                threads.AddFirst(provider);
            }
            for (int i = 0; i < numOfWorkers; i++)
            {
                Thread worker = new Thread(workers[i].ExecuteTasks);
                threads.AddFirst(worker);
            }
            foreach (Thread t in threads){
                t.Start();
            }
            
            Thread.Sleep(500);

            foreach (Thread t in threads)
                t.Join();

            return tasks.GetLogs();
        }
    }
}
