using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Gender
{
    public char GenderId { get; set; }

    public string Gender1 { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
