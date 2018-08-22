using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace Kontur.ImageTransformer
{
    internal abstract class AsyncHttpServer : IDisposable
    {
        public AsyncHttpServer()
        {
            listener = new HttpListener();

            System.Timers.Timer timerForPercentageCalculation = CreateTimerToCalculateProcentageOfOutOfTimeRequests();
            timerForPercentageCalculation.Start();
        }

        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();
                    
                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }

        protected void Listen()
        {
            while (true)
            {
                try
                {
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();

                        if (tooManyRequests)
                        {
                            context.Response.StatusCode = TOO_MANY_REQUESTS;
                            context.Response.Close();
                        }
                        else
                        {
                            Task handleContextTask = Task.Run(() => HandleContext(context));

                            System.Timers.Timer timer = CreateHandleContextTimer(handleContextTask, context);
                            timer.Start();
                        }   
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
            }
        }

        private System.Timers.Timer CreateTimerToCalculateProcentageOfOutOfTimeRequests()
        {
            System.Timers.Timer timerForPercentageCalculation =
                new System.Timers.Timer(TIME_TO_CALCULATE_PERCENTAGE_OF_OUT_OF_TIME_REQUESTS);
            timerForPercentageCalculation.Elapsed += RecalculatePercentageOfOutOfTimeRequests;
            return timerForPercentageCalculation;
        }

        private void RecalculatePercentageOfOutOfTimeRequests(object obj, ElapsedEventArgs args)
        {
            requestsOutOfTimePerSecondInPercents =
                (float)requestsOutOfTimePerSecond / (requestsOutOfTimePerSecond + requestCompletedPerSecond);
            if (requestsOutOfTimePerSecondInPercents > MAX_PERCENTAGE_OF_OUT_OF_TIME_REQUEST)
            {
                tooManyRequests = true;
            }
            else
            {
                tooManyRequests = false;
            }
            requestCompletedPerSecond = 0;
            requestsOutOfTimePerSecond = 0;
        }

        private System.Timers.Timer CreateHandleContextTimer(Task handleContextTask, HttpListenerContext context)
        {
            System.Timers.Timer handleContextTimer = new System.Timers.Timer(TIME_TO_HANDLE_CONTEXT);
            handleContextTimer.Elapsed += (obj, args) =>
            {
                if (handleContextTask.IsCompleted)
                {
                    requestCompletedPerSecond++;
                }
                else
                {
                    requestsOutOfTimePerSecond++;
                    context.Response.StatusCode = TOO_MANY_REQUESTS;
                    context.Response.Close();
                    handleContextTimer.Stop();
                    handleContextTask.Dispose();
                }
            };
            return handleContextTimer;
        }

        protected abstract void HandleContext(HttpListenerContext listenerContext);

        private const int TIME_TO_CALCULATE_PERCENTAGE_OF_OUT_OF_TIME_REQUESTS = 1000;
        private const int TIME_TO_HANDLE_CONTEXT = 1000;
        private const float MAX_PERCENTAGE_OF_OUT_OF_TIME_REQUEST = 0.1f;

        private const int TOO_MANY_REQUESTS = 429;
        private const int BAD_REQUEST = 400;

        private volatile int requestCompletedPerSecond;
        private volatile int requestsOutOfTimePerSecond;
        private float requestsOutOfTimePerSecondInPercents;

        private volatile bool tooManyRequests;

        private readonly HttpListener listener;
        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
    }
}