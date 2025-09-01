namespace OurCraft.utility
{
    //allows to safely run functions on seperate threads
    public class ThreadPoolSystem : IDisposable
    {
        //---explanation---

        //thread pool contains the list of events - task queue
        //and a list of threads that can run those events - workers
        //there are only a set amount of threads that can run functions concurrently all at once
        //this is so that the cpu doesn't have a stroke with too many threads
        //once a thread is done, it can become available for the next task queued up

        //threading data
        private readonly List<Thread> workers;
        private readonly Queue<Action> taskQueue;

        //thread safety
        private readonly object queueLock = new();
        private volatile bool stopFlag = false;

        //create a list of set threads and start their worker loop
        public ThreadPoolSystem(int threadCount)
        {
            workers = new List<Thread>(threadCount);
            taskQueue = new Queue<Action>();

            for (int i = 0; i < threadCount; i++)
            {
                //create thread and run the worker loop
                var worker = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    Name = $"ThreadPoolWorker_{i}"
                };
                workers.Add(worker);
                worker.Start();
            }
        }

        //methods
        //worker loop for each thread
        private void WorkerLoop()
        {
            while (!stopFlag)
            {
                Action task = null!;

                lock (queueLock)
                {
                    while (!stopFlag && taskQueue.Count == 0)
                    {
                        Monitor.Wait(queueLock);
                    }

                    if (stopFlag && taskQueue.Count == 0)
                        return;

                    task = taskQueue.Dequeue();
                }

                task();
            }
        }

        //adds a task to the task queue safely
        public void Submit(Action task)
        {
            if (stopFlag)
                return;

            lock (queueLock)
            {
                taskQueue.Enqueue(task);
                //wake up a thread for new task
                Monitor.Pulse(queueLock); 
            }
        }

        //join all threads back to the main thread
        public void Stop()
        {
            stopFlag = true;
            lock (queueLock)
            {
                //wake up all threads to exit
                Monitor.PulseAll(queueLock);
            }

            foreach (var thread in workers)
            {
                if (thread.IsAlive)
                    thread.Join();
            }
        }

        //stop
        public void Dispose()
        {
            Stop();
        }
    }
}