using System.Collections.Generic;
using System.Linq;

namespace OneExeProgram.Domain
{
    public class Order
    {
        private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>( );
        public Customer Customer { get; set; }
        public OrderLineItem[] GetOrderLineItems( )
        {
            return _orderLineItems.ToArray( );
        }
        public void AddOrderLineItem( Product product, int quantity )
        {
            _orderLineItems.Add( new OrderLineItem( product, quantity ) );
        }
        public decimal GetTotal( )
        {
            return _orderLineItems.Sum( li => li.GetTotal( ) );
        }
    }
}
