namespace NexCart.Domain
{
    public class Role : BaseEntity
    {
        private string _name;
        public virtual string Name
        {
            get => _name;
            set => _name = value;
        }
    }
}
