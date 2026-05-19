using System.ComponentModel.DataAnnotations;
using System.ComponentModel;


namespace InventoryManagement.Models
{
    public class Order    
    {
        [Key]
        public int OrderId { get; set; }

        [Required, MaxLength(100), RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Customer name can only contain letters and spaces.")]
        public string CustomerName { get; set; }

        // Set default value to current date and time when the order is created
        [DefaultValue(typeof(DateTime), "DateTime.Now")]
        public DateTime DatePlaced { get; set; }

        // Navigation property to link orders with inventory items
        public ICollection<InventoryItem> Items { get; set; }

        public void GetOrderSummary()
        {
            Console.WriteLine($"Order ID: {OrderId}, Customer: {CustomerName}, Date: {DatePlaced}");
            Console.WriteLine("Items in Order:");
            if (Items != null)
            {
                foreach (var item in Items)
                {
                    item.DisplayInfo();
                }
            }
            else
            {
                Console.WriteLine("No items in this order.");
            }
        }

        public void AddItem(InventoryItem item)
        {
            if (Items == null)
            {
                // Initialize the collection if it's null before adding items
                Items = new List<InventoryItem>();
            }
            Items.Add(item);
        }

        public void RemoveItem(int ItemId)
        {
            var item = Items?.FirstOrDefault(i => i.ItemId == ItemId);

            if (item != null)
            {
                Items.Remove(item);
            }
        }
    }
}