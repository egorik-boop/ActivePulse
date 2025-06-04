using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int ManufacturerId { get; set; }

    public int CategoryName { get; set; }

    public int WarrantyPeriodInMonth { get; set; }

    public virtual ProductCategory CategoryNameNavigation { get; set; } = null!;

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
