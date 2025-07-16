using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivePulse.Data
{
    public class Cart
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty; // Инициализация по умолчанию
        public string ImagePath { get; set; } = string.Empty; // Инициализация по умолчанию
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; }
        public decimal Total => Price * Quantity;
    }
}
