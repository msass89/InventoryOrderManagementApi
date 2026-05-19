using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace InventoryManagement.Models
{
    public class InventoryItem
    {   
        [Key]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [DefaultValue(0)]
        public int Quantity { get; set; }

        [MaxLength(100)]
        public string Location { get; set; }

        public void DisplayInfo()
        {
            Console.WriteLine($"ID: {ItemId}, Name: {Name}, Quantity: {Quantity}, Location: {Location}");
        }
    }
}