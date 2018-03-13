using System.Collections.Generic;

namespace Kontur.ImageTransformer
{
    class TimeEstimator
    {
        public void UpdateAverage(long newTime)
        {
            if (numberOfRequests < AmountOfResponcesToStore)
            {
                queue.Enqueue(newTime);

                sumResponcesTime += newTime;
                numberOfRequests++;
            }
            else
            {
                //this request was long time ago, it almost doesn't affect, so we can don't include it in estimation
                long oldTime = queue.Dequeue();

                sumResponcesTime -= oldTime;
                sumResponcesTime += newTime;

                queue.Enqueue(newTime);
            }
        }

        private long sumResponcesTime;
        private long numberOfRequests;

        private const int AmountOfResponcesToStore = 100;

        //store AmountOfResponcesToStore or less most recent response time estimations
        private Queue<long> queue = new Queue<long>();

        public long AverageResponseTime => numberOfRequests == 0 ? 0 : sumResponcesTime / numberOfRequests;
    }
}
