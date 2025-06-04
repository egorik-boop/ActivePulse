using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Supply
{
    public int SupplyId { get; set; }

    public int StoreId { get; set; }

    public DateOnly SupplyDate { get; set; }

    public virtual Store Store { get; set; } = null!;
}
