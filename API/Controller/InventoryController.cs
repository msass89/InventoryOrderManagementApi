using Microsoft.AspNetCore.Mvc;
using InventoryManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private const string CacheKeyInventoryItems = "inventory_items_all";
    private const string CacheKeyInventoryItemPrefix = "inventory_item_";

    public InventoryController(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    //get all inventory items
    [HttpGet]
    [Authorize(Roles = "Admin, InventoryAgent")]
    public async Task<IActionResult> GetInventoryItems()
    {
        if (!_memoryCache.TryGetValue(CacheKeyInventoryItems, out List<ItemResponseDto>? cachedItems))
        {
            Console.WriteLine("Cache miss: Retrieving inventory items from database and caching them.");

            // If the cache does not contain the inventory items, retrieve them from the database
            var items = await _context.InventoryItems.Select(item => new ItemResponseDto
            {
                InventoryItemId = item.ItemId,
                ItemName = item.Name,
                QuantityInStock = item.Quantity,
                Location = item.Location
            }).ToListAsync();

            // Set cache options
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)); // Cache for 30 seconds

            // Save data in cache
            _memoryCache.Set(CacheKeyInventoryItems, items, cacheEntryOptions);
            cachedItems = items;
        }
        else
        {
            Console.WriteLine("Cache hit: Retrieving inventory items from cache.");
        }

        return Ok(cachedItems);
    }


    //get a specific inventory item by id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, InventoryAgent")]
    public async Task<IActionResult> GetInventoryItem(int id)
    {
        if(!_memoryCache.TryGetValue(CacheKeyInventoryItemPrefix + id, out ItemResponseDto? cachedItem))
        {
            Console.WriteLine($"Cache miss: Retrieving inventory item with ID {id} from database.");

            // get the database context from the request services
            var itemResponseDto = await _context.InventoryItems
                .Where(i => i.ItemId == id)
                .Select(item => new ItemResponseDto
                {
                    InventoryItemId = item.ItemId,
                    ItemName = item.Name,
                    QuantityInStock = item.Quantity,
                    Location = item.Location
                })
                .FirstOrDefaultAsync();

            if (itemResponseDto == null)
            {
                return NotFound();
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)); // Cache for 30 seconds

            // Save data in cache
            _memoryCache.Set(CacheKeyInventoryItemPrefix + id, itemResponseDto, cacheEntryOptions);
            cachedItem = itemResponseDto;
        }
        else
        {
            Console.WriteLine($"Cache hit: Retrieving inventory item with ID {id} from cache.");
        }
        
        return Ok(cachedItem);
    }

    //create a new inventory item
    [HttpPost]
    [Authorize(Roles = "Admin, InventoryAgent")]
    public async Task<IActionResult> CreateInventoryItem([FromBody] CreateItemDto createItemDto)
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
        _context.InventoryItems.Add(inventoryItem);
        await _context.SaveChangesAsync();

        // return the created item as a DTO
        ItemResponseDto itemResponseDto = new ItemResponseDto
        {
            InventoryItemId = inventoryItem.ItemId,
            ItemName = inventoryItem.Name,
            QuantityInStock = inventoryItem.Quantity,
            Location = inventoryItem.Location
        };

        // invalidate cached inventory list after creating a new item
        _memoryCache.Remove(CacheKeyInventoryItems);

        return CreatedAtAction(nameof(GetInventoryItem), new { id = inventoryItem.ItemId }, itemResponseDto);
    }

    //delete an inventory item by id
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, InventoryAgent")]
    public async Task<IActionResult> DeleteInventoryItem(int id)
    {
        var item = await _context.InventoryItems.FirstOrDefaultAsync(item => item.ItemId == id);
        if (item == null)
        {
            return NotFound();
        }
        _context.InventoryItems.Remove(item);
        await _context.SaveChangesAsync();

        // invalidate cached inventory list after deleting an item
        _memoryCache.Remove(CacheKeyInventoryItems);
        
        return Ok($"Inventory item with ID {id} deleted successfully.");
    }
}