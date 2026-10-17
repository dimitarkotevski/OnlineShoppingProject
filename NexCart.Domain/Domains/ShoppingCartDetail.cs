namespace NexCart.Domain
{
    public class ShoppingCartDetail : BaseEntity
    {
        private ShoppingCart _shoppingCart;
        private Product _product;
        private int _quantity;

        public virtual ShoppingCart ShoppingCart 
        { 
            get => _shoppingCart;
            set => _shoppingCart = value; 
        }
        public virtual Product Product
        {
            get => _product;
            set => _product = value;
        }
        public virtual int Quantity
        {
            get => _quantity;
            set => _quantity = value;
        }
    }
}
