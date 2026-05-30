
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    // DTO for creating a new order, including customer name and list of order items
    public class CreateOrderDto
    {
        [Required, MaxLength(100)]
        public string CustomerName { get; set; }
        [Required]
        public List<OrderItemDto> OrderItemDto { get; set; }
    }

    // DTO for individual order items in the CreateOrderDto
    public class OrderItemDto
    {
        [Required]
        public int InventoryItemId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}