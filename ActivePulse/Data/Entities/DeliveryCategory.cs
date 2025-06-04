using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class DeliveryCategory
{
    public int DeliveryCategoryId { get; set; }

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();
}
