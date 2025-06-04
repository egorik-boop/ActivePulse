using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class PayOrder
{
    public int OrderId { get; set; }

    public string Card { get; set; } = null!;

    public int PayStatus { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual PayStatus PayStatusNavigation { get; set; } = null!;
}
