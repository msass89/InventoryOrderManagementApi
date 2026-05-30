using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Models;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly ApplicationDbContext context;
    
    public OrderController(ApplicationDbContext context)
    {
        this.context = context;
    }

    //get all orders
    [HttpGet]
    public IActionResult GetOrders()
    {
        // get the database context from the request services and return a list of DTOs
        List<OrderResponseDto> orders = context.Orders
            .Select(o => new OrderResponseDto
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                DatePlaced = o.DatePlaced,
                // Map order items to their corresponding DTOs, including item names
                OrderItemResponseDto = o.OrderItems.Select(orderItem => new OrderItemResponseDto
                {
                    InventoryItemId = orderItem.InventoryItemId,
                    ItemName = orderItem.InventoryItem != null ? orderItem.InventoryItem.Name : null,
                    Quantity = orderItem.Quantity
                }).ToList()
            })
            .ToList();
        return Ok(orders);
    }

    //get a specific order by id
    [HttpGet("{id}")]
    public IActionResult GetOrder(int id)
    {
        // get the database context from the request services and return a DTO
        OrderResponseDto? order = context.Orders
            .Where(o => o.OrderId == id)
            .Select(o => new OrderResponseDto
            {
                OrderId = o.OrderId,
                CustomerName = o.CustomerName,
                DatePlaced = o.DatePlaced,
                OrderItemResponseDto = o.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    InventoryItemId = oi.InventoryItemId,
                    ItemName = oi.InventoryItem != null ? oi.InventoryItem.Name : null,
                    Quantity = oi.Quantity
                }).ToList()
            })
            .FirstOrDefault();
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    //create a new order with validation 
    [HttpPost]
    public IActionResult CreateOrder([FromBody] CreateOrderDto createOrderDto)
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
        var inventoryItems = context.InventoryItems.Where(i => itemIds.Contains(i.ItemId)).ToList();
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

        context.Orders.Add(order);
        context.SaveChanges();

        // Return DTO
        OrderResponseDto orderResponseDto = new OrderResponseDto
        {
            OrderId = order.OrderId,
            CustomerName = order.CustomerName,
            DatePlaced = order.DatePlaced,
            OrderItemResponseDto = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                InventoryItemId = oi.InventoryItemId,
                ItemName = inventoryItems.FirstOrDefault(ii => ii.ItemId == oi.InventoryItemId)?.Name,
                Quantity = oi.Quantity
            }).ToList()
        };
        return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, orderResponseDto);
    }

    //delete an order by id
    [HttpDelete("{id}")]
    public IActionResult DeleteOrder(int id)
    {
        var order = context.Orders.FirstOrDefault(o => o.OrderId == id);
        if (order == null)
        {
            return NotFound();
        }

        context.Orders.Remove(order);
        
        context.SaveChanges();
        return Ok($"Order with ID {id} deleted successfully.");
    }
}