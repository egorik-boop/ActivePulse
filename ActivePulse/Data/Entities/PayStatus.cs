using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class PayStatus
{
    public int PayStatusId { get; set; }

    public string StatusDescription { get; set; } = null!;
}
