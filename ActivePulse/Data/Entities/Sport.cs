using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Sport
{
    public int SportId { get; set; }

    public string SportName { get; set; } = null!;

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}
