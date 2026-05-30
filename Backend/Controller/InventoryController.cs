using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext context;

    public InventoryController(ApplicationDbContext context)
    {
        this.context = context;
    }

    //get all inventory items
    [HttpGet]
    public IActionResult GetInventoryItems()
    {
        return Ok(context.InventoryItems.Select(item => new ItemResponseDto
        {
            InventoryItemId = item.ItemId,
            ItemName = item.Name,
            QuantityInStock = item.Quantity,
            Location = item.Location
        }).ToList());
    }

    //get a specific inventory item by id
    [HttpGet("{id}")]
    public IActionResult GetInventoryItem(int id)
    {
        // get the database context from the request services
        ItemResponseDto? itemResponseDto = context.InventoryItems
            .Where(i => i.ItemId == id)
            .Select(item => new ItemResponseDto
            {
                InventoryItemId = item.ItemId,
                ItemName = item.Name,
                QuantityInStock = item.Quantity,
                Location = item.Location
            })
            .FirstOrDefault();
        if (itemResponseDto == null)
        {
            return NotFound();
        }
        return Ok(itemResponseDto);
    }

    //create a new inventory item
    [HttpPost]
    public IActionResult CreateInventoryItem([FromBody] CreateItemDto createItemDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var inventoryItem = new InventoryItem
        {
            Name = createItemDto.Name,
            Quantity = createItemDto.Quantity,
            Location = createItemDto.Location
        };
        context.InventoryItems.Add(inventoryItem);
        context.SaveChanges();

        // return the created item as a DTO
        ItemResponseDto itemResponseDto = new ItemResponseDto
        {
            InventoryItemId = inventoryItem.ItemId,
            ItemName = inventoryItem.Name,
            QuantityInStock = inventoryItem.Quantity,
            Location = inventoryItem.Location
        };
        return CreatedAtAction(nameof(GetInventoryItem), new { id = inventoryItem.ItemId }, itemResponseDto);
    }

    //delete an inventory item by id
    [HttpDelete("{id}")]
    public IActionResult DeleteInventoryItem(int id)
    {
        var item = context.InventoryItems.FirstOrDefault(item => item.ItemId == id);
        if (item == null)
        {
            return NotFound();
        }
        context.InventoryItems.Remove(item);
        context.SaveChanges();
        return Ok($"Inventory item with ID {id} deleted successfully.");
    }
}