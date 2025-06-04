using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Store
{
    public int StoreId { get; set; }

    public string StoreAddress { get; set; } = null!;

    public TimeOnly OpeningTime { get; set; }

    public TimeOnly ClosingTime { get; set; }

    public string Phone { get; set; } = null!;

    public string? StoreName { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Supply> Supplies { get; set; } = new List<Supply>();
}
