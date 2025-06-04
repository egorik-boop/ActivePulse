using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class ProductCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int SportId { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Sport Sport { get; set; } = null!;
}
