namespace OneExeProgram.Domain
{
    public class OrderLineItem
    {
        public OrderLineItem( Product product, int quantity )
        {
            Product = product;
            Quantity = quantity;
        }
        public Product Product { get; private set; }
        public int Quantity { get; private set; }
        public decimal GetTotal( )
        {
            return Quantity * Product.Price;
        }
    }
}
