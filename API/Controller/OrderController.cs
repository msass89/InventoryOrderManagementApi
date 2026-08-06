using Microsoft.AspNetCore.Mvc;
using InventoryManagementApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _memoryCache;
     private const string CacheKeyOrders = "orders_all";
    private const string CacheKeyOrderPrefix = "order_";
    
    public OrderController(ApplicationDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }

    //get all orders
    [HttpGet]
    [Authorize(Roles = "Admin, SalesAgent")]
    public async Task<IActionResult> GetOrders()
    {
        if(!_memoryCache.TryGetValue(CacheKeyOrders, out List<OrderResponseDto>? cachedOrders))
        {
            Console.WriteLine("Cache miss: Retrieving orders from database and caching them.");

            // If the cache does not contain the orders, retrieve them from the database
            cachedOrders = await _context.Orders
                .Include(o => o.OrderItems) // Include the related OrderItems
                    .ThenInclude(oi => oi.InventoryItem) // Include the related InventoryItem for each OrderItem
                .Select(o => new OrderResponseDto // Map each Order to an OrderResponseDto
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    DatePlaced = o.DatePlaced,
                    // Map order items to their corresponding DTOs, including item names
                    OrderItemResponseDto = o.OrderItems.Select(orderItem => new OrderItemResponseDto
                    {
                        InventoryItemId = orderItem.InventoryItemId,
                        ItemName = orderItem.InventoryItem != null ? orderItem.InventoryItem.Name : string.Empty,
                        Quantity = orderItem.Quantity
                    }).ToList()
                }).ToListAsync();

            // Set cache options
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)); // Cache for 30 seconds

            // Save data in cache
            _memoryCache.Set(CacheKeyOrders, cachedOrders, cacheEntryOptions);
        }
        else
        {
            Console.WriteLine("Cache hit: Retrieving orders from cache.");
        }
        // get the database context from the request services and return a list of DTOs
        /*List<OrderResponseDto> orders = await _context.Orders
            .Include(o => o.OrderItems) // Include the related OrderItems
                .ThenInclude(oi => oi.InventoryItem) // Include the related InventoryItem for each OrderItem
            .Select(o => new OrderResponseDto // Map each Order to an OrderResponseDto
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                DatePlaced = o.DatePlaced,
                // Map order items to their corresponding DTOs, including item names
                OrderItemResponseDto = o.OrderItems.Select(orderItem => new OrderItemResponseDto
                {
                    InventoryItemId = orderItem.InventoryItemId,
                    ItemName = orderItem.InventoryItem != null ? orderItem.InventoryItem.Name : string.Empty,
                    Quantity = orderItem.Quantity
                }).ToList()
            }).ToListAsync();*/
            
        return Ok(cachedOrders);
    }

    //get a specific order by id
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin, SalesAgent")]
    public async Task<IActionResult> GetOrder(int id)
    {
        if(!_memoryCache.TryGetValue(CacheKeyOrderPrefix + id, out OrderResponseDto? cachedOrder))
        {
            Console.WriteLine($"Cache miss: Retrieving order with ID {id} from database and caching it.");

            // get the database context from the request services and return a DTO
            OrderResponseDto? order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.InventoryItem)
                .Where(o => o.OrderId == id)
                .Select(o => new OrderResponseDto
                {
                    OrderId = o.OrderId,
                    CustomerName = o.CustomerName,
                    DatePlaced = o.DatePlaced,
                    OrderItemResponseDto = o.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        InventoryItemId = oi.InventoryItemId,
                        ItemName = oi.InventoryItem != null ? oi.InventoryItem.Name : string.Empty,
                        Quantity = oi.Quantity
                    }).ToList()
                })
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return NotFound();
            }

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30)); // Cache for 30 seconds
            
            // Save data in cache
            _memoryCache.Set(CacheKeyOrderPrefix + id, order, cacheEntryOptions);
            cachedOrder = order;
        }
        else
        {
            Console.WriteLine($"Cache hit: Retrieving order with ID {id} from cache.");
        }
        
        return Ok(cachedOrder);
    }

    //create a new order with validation 
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
    {
        // Validate the incoming DTO 
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Ensure the order contains at least one item
        if (createOrderDto.OrderItemDto == null || !createOrderDto.OrderItemDto.Any())
        {
            return BadRequest("Order must contain at least one item.");
        }

        // Fetch all referenced inventory items and validate their existence
        var itemIds = createOrderDto.OrderItemDto.Select(i => i.InventoryItemId).ToList();
        var inventoryItems = await _context.InventoryItems.Where(i => itemIds.Contains(i.ItemId)).ToListAsync();
        if (inventoryItems.Count != itemIds.Count)
        {
            return BadRequest("One or more item IDs do not exist.");
        }

        // Create order items
        var orderItems = createOrderDto.OrderItemDto.Select(i => new OrderItem
        {
            InventoryItemId = i.InventoryItemId,
            Quantity = i.Quantity
        }).ToList();

        var order = new Order
        {
            CustomerName = createOrderDto.CustomerName,
            DatePlaced = DateTime.UtcNow,
            OrderItems = orderItems
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Return DTO
        OrderResponseDto orderResponseDto = new OrderResponseDto
        {
            OrderId = order.OrderId,
            CustomerName = order.CustomerName,
            DatePlaced = order.DatePlaced,
            OrderItemResponseDto = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                InventoryItemId = oi.InventoryItemId,
                ItemName = inventoryItems.FirstOrDefault(ii => ii.ItemId == oi.InventoryItemId)?.Name ?? string.Empty,
                Quantity = oi.Quantity
            }).ToList()
        };

        // invalidate cached order list after creating a new order
        _memoryCache.Remove(CacheKeyOrders);

        return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, orderResponseDto);
    }

    //delete an order by id
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin, SalesAgent, Customer")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);
        if (order == null)
        {
            return NotFound();
        }

        _context.Orders.Remove(order);
        
        await _context.SaveChangesAsync();

        // invalidate cached order list after creating a new order
        _memoryCache.Remove(CacheKeyOrders);

        return Ok($"Order with ID {id} deleted successfully.");
    }
}