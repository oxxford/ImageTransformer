using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    public class ProducerConsumerQueue : IDisposable
    {
        public ProducerConsumerQueue(int workerCount)
        {
            // Create and start a separate Task for each consumer:
            for (int i = 0; i < workerCount; i++)
                Task.Factory.StartNew(Consume);
        }

        public void Dispose()
        {
            contextQueue.CompleteAdding();
        }

        public void Enqueue(HttpListenerContext context)
        {
            bool ifDiscard = false;
            long startResponseTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            lock (timeEstimator)
            {
                if (timeEstimator.AverageResponseTime * contextQueue.Count > 1000)
                    ifDiscard = true;
            }

            //if too many requests just don't handle next
            if (ifDiscard)
                DiscardContext(context);
            else
            {
                contextQueue.Add(new HttpContextNode(context, startResponseTime));
            }
        }

        void Consume()
        {
            // This sequence will block when no elements are available and will end when CompleteAdding is called. 
            foreach (HttpContextNode node in contextQueue.GetConsumingEnumerable())
            {
                HandleContext(node.Context);

                lock (timeEstimator)
                {
                    long finishResponseTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    timeEstimator.UpdateAverage(finishResponseTime - node.StartResponseTime);
                }
            }
        }

        void HandleContext(HttpListenerContext context)
        {
            try
            {
                ContextHandler.HandleContext(context);
                context.Response.StatusCode = Ok;
                context.Response.Close();
            }
            catch (ArgumentException)
            {
                context.Response.StatusCode = BadRequest;
                context.Response.Close();
            }
            catch (EmptyResponseException)
            {
                context.Response.StatusCode = EmptyResponse;
                context.Response.Close();
            }
            //logging for worker threads
            catch (Exception error)
            {
                Trace.TraceError(error.Message);
                context.Response.StatusCode = ServerError;
                context.Response.Close();
            }
        }

        void DiscardContext(HttpListenerContext context)
        {
            context.Response.StatusCode = TooManyRequests;
            context.Response.Close();
        }

        private BlockingCollection<HttpContextNode> contextQueue = new BlockingCollection<HttpContextNode>();

        //This is used to predict probable time for one response
        private TimeEstimator timeEstimator = new TimeEstimator();

        private const int TooManyRequests = 429;
        private const int BadRequest = 400;
        private const int Ok = 200;
        private const int EmptyResponse = 204;
        private const int ServerError = 503;
    }
}
