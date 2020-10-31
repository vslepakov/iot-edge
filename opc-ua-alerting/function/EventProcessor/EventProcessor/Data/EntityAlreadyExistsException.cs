using System;

namespace EventProcessor.Data
{
    public class EntityAlreadyExistsException : Exception
    {
        public EntityAlreadyExistsException() { }

        public EntityAlreadyExistsException(string message) : base(message) { }
    }
}
