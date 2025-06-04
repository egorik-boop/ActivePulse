using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Country
{
    public string CountryCode { get; set; } = null!;

    public string CountryName { get; set; } = null!;

    public virtual ICollection<Manufacturer> Manufacturers { get; set; } = new List<Manufacturer>();
}
