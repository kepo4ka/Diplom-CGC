using AutoMapper;

namespace OneExeProgram.Domain
{
    /// <summary>
    /// Пример использования AutoMapper
    /// http://automapper.codeplex.com/wikipage?title=Flattening
    /// </summary>
    public class AutoMapperTester
    {
        /// <summary>
        /// Работает ли AutoMapper
        /// </summary>
        public bool IsCorrect
        {
            get
            {
                // Complex model
                var customer = new Customer
                {
                    Name = "George Costanza"
                };
                var order = new Order
                {
                    Customer = customer
                };
                var bosco = new Product
                {
                    Name = "Bosco",
                    Price = 4.99m
                };
                order.AddOrderLineItem( bosco, 15 );

                // Configure AutoMapper
                Mapper.CreateMap<Order, OrderDto>( );

                // Perform mapping
                OrderDto dto = Mapper.Map<Order, OrderDto>( order );
                var isCorrect =
                    ( dto.CustomerName == "George Costanza" ) &&
                    ( dto.Total == 74.85m );
                return isCorrect;
            }
        }
    }
}
