using System.ComponentModel.DataAnnotations;

public class ItemResponseDto
{
    [Key]
    public int InventoryItemId { get; set; }
    public string ItemName { get; set; }
    public int QuantityInStock { get; set; }
    public string Location { get; set; }
}