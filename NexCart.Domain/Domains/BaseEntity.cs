using System;

namespace NexCart.Domain
{
    public class BaseEntity
    {
        private Guid _id;
        public virtual Guid Id { get { return _id; } set { _id = value; } }
    }
}
