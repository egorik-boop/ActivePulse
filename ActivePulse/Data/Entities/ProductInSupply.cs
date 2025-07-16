using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class ProductInSupply
{
    public int SupplyId { get; set; }
    public int ProductId { get; set; }
    public int Amount { get; set; }
    public int Price { get; set; }
    public string ProductSize { get; set; } // Добавьте это свойство

    public virtual Product Product { get; set; } = null!;
}   
