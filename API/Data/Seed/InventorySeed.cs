using InventoryManagementApi.Models;

public static class InventorySeed
{
    public static async Task SeedInventoryAsync(IServiceProvider services)
    {
        using (var context = services.GetRequiredService<ApplicationDbContext>())
        {
            // Add test inventory item if none exist
            if (!context.InventoryItems.Any())
            {
                context.InventoryItems.Add(new InventoryItem
                {
                    Name = "Pallet Jack",
                    Quantity = 12,
                    Location = "WarehouseA"
                });

                context.SaveChanges();
            }

            // Retrieve and print inventory to confirm
            var items = context.InventoryItems.ToList();
            foreach (var item in items)
            {
                item.DisplayInfo(); // Should print: Item: Pallet Jack | Quantity: 12 | Location: WarehouseA
            }
        }
    }
}