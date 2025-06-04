using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public string? Description { get; set; }

    public virtual Product Product { get; set; } = null!;
}
