using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class OrderStatus
{
    public int OrderStatusId { get; set; }

    public string OrderStatusDescription { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
