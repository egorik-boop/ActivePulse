using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Patronymic { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public char? Gender { get; set; }

    public string? Phone { get; set; }

    public virtual Gender? GenderNavigation { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
