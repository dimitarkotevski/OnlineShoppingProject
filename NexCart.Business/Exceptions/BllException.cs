using System;

namespace NexCart.Business.Exceptions
{
    public class BllException : Exception
    {
        public BllException(string message) : base(message)
        {
        }
    }
}
