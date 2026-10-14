using System;

namespace IntellaQuest.BusinessLogic.Exceptions
{
    public class BllException : Exception
    {
        public BllException(string message) : base(message)
        {
        }
    }
}
