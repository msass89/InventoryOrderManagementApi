using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryManagementApi.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        // Foreign key to link OrderItem with Order
        [Required]
        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        // Foreign key to link OrderItem with InventoryItem
        [Required]
        public int InventoryItemId { get; set; }
        [ForeignKey("InventoryItemId")]
        public InventoryItem InventoryItem { get; set; }

        [Required]
        public int Quantity { get; set; }

        public void DisplayInfo()
        {
            Console.WriteLine($"OrderItem ID: {OrderItemId}, Inventory Item ID: {InventoryItemId}, Quantity: {Quantity}");
        }
    }



}