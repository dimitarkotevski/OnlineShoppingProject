namespace NexCart.Domain
{
    /// <summary>
    /// Immutable snapshot of one product line at checkout. Order history is built from these
    /// rows, never from the live shopping cart, so later price/name changes cannot rewrite it.
    /// </summary>
    public class OrderItem : BaseEntity
    {
        private Order _order;
        private Product _product;
        private string _productName;
        private decimal _unitPrice;
        private int _quantity;

        public virtual Order Order
        {
            get => _order;
            set => _order = value;
        }

        /// <summary>
        /// Null when the catalogue product has since been deleted. The name/price snapshot and
        /// quantity remain available for order history.
        /// </summary>
        public virtual Product Product
        {
            get => _product;
            set => _product = value;
        }

        public virtual string ProductName
        {
            get => _productName;
            set => _productName = value;
        }

        public virtual decimal UnitPrice
        {
            get => _unitPrice;
            set => _unitPrice = value;
        }

        public virtual int Quantity
        {
            get => _quantity;
            set => _quantity = value;
        }
    }
}
