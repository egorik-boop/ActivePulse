using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Delivery
{
    public int DeliveryId { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public int OrderId { get; set; }

    public int DeliveryCategoryId { get; set; }

    public string Address { get; set; } = null!;

    public virtual DeliveryCategory DeliveryCategory { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
