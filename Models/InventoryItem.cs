using System;

namespace EcommerceAPI.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdated { get; set; }
    }
} 