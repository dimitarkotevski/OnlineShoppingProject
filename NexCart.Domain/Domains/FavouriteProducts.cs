namespace NexCart.Domain
{
    public class FavouriteProducts : BaseEntity
    {
        private User _user;
        private Product _product;

        public FavouriteProducts() { }
        public virtual User User
        {
            get => _user;
            set => _user = value;
        }
        public virtual Product Product
        {
            get => _product;
            set => _product = value;
        }
    }
}
