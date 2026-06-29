using System;
using System.Collections.Generic;

namespace InventoryManagementApi.Models
{
    // DTO for returning order details
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? DatePlaced { get; set; }
        public List<OrderItemResponseDto> OrderItemResponseDto { get; set; }
    }

    // DTO for individual order items in the OrderResponseDto
    public class OrderItemResponseDto
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }
}