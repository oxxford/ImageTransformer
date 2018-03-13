using System;

namespace Kontur.ImageTransformer
{
    class EmptyResponseException: Exception
    {
        public EmptyResponseException()
        {
        }

        public EmptyResponseException(string message)
            : base(message)
        {
        }

        public EmptyResponseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
