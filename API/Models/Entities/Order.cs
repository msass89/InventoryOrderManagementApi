using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace InventoryManagementApi.Models
{
    public class Order    
    {
        [Key]
        public int OrderId { get; set; }

        [Required, MaxLength(100), RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Customer name can only contain letters and spaces.")]
        public string CustomerName { get; set; }

        // Set default value to current date and time when the order is created
        [DefaultValue(typeof(DateTime), "DateTime.Now")]
        public DateTime? DatePlaced { get; set; }

        // Navigation property to link orders with order items
        public ICollection<OrderItem> OrderItems { get; set; }

        public void GetOrderSummary()
        {
            Console.WriteLine($"Order ID: {OrderId}, Customer: {CustomerName}, Date: {DatePlaced}");
            Console.WriteLine("Items in Order:");
            if (OrderItems != null)
            {
                foreach (var item in OrderItems)
                {
                    item.DisplayInfo();
                }
            }
            else
            {
                Console.WriteLine("No items in this order.");
            }
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            if (OrderItems == null)
            {
                // Initialize the collection if it's null before adding items
                OrderItems = new List<OrderItem>();
            }
            OrderItems.Add(orderItem);
        }

        public void RemoveItem(int ItemId)
        {
            var item = OrderItems?.FirstOrDefault(i => i.InventoryItemId == ItemId);

            if (item != null)
            {
                OrderItems.Remove(item);
            }
        }
    }
}