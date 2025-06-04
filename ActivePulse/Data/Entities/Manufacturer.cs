using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Manufacturer
{
    public int ManufacturerId { get; set; }

    public string ManufacturerName { get; set; } = null!;

    public string CountryCode { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual Country CountryCodeNavigation { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
