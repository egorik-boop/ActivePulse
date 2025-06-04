using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivePulse.Data
{
    public class Favourite
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImagePath { get; set; } = String.Empty;
        public decimal Price { get; set; }
    }
}
