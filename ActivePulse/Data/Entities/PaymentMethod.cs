using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
