using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    // DTO for creating a new inventory item, including name and quantity
    public class CreateItemDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative integer.")]
        public int Quantity { get; set; }

        //location should be one of the predefined values: "Warehouse A", "Warehouse B", "Warehouse C"
        [Required (ErrorMessage = "Location is required.")]
        [EnumDataType(typeof(InventoryLocation), ErrorMessage = "Location must be one of the following: WarehouseA, WarehouseB, WarehouseC.")]
        public string Location { get; set; }
    }
}