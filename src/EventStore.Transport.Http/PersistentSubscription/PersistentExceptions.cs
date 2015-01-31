using System;
using System.Runtime.Serialization;

namespace EventStore.Transport.Http.PersistentSubscription
{
    public class PersistentSpecificationViolationException : Exception
    {
        public PersistentSpecificationViolationException()
        {
        }

        public PersistentSpecificationViolationException(string message) : base(message)
        {
        }

        public PersistentSpecificationViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PersistentSpecificationViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class ThrowHelper
    {
        public static void ThrowSpecificationViolation(string message)
        {
            throw new PersistentSpecificationViolationException(message);
        }

        public static void ThrowSpecificationViolation(string message, Exception innnerException)
        {
            throw new PersistentSpecificationViolationException(message, innnerException);
        }
    }
}