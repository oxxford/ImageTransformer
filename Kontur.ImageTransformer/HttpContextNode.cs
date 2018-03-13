using System.Net;

namespace Kontur.ImageTransformer
{
    public class HttpContextNode
    {
        private HttpListenerContext context;
        private long startResponseTime;

        public HttpListenerContext Context => context;

        public long StartResponseTime => startResponseTime;

        public HttpContextNode(HttpListenerContext context, long startResponseTime)
        {
            this.context = context;
            this.startResponseTime = startResponseTime;
        }
    }
}
