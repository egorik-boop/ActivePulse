using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class User
{
    public int UserId { get; set; }

    public int? EmployeeId { get; set; }

    public int? CustomerId { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public char? RoleId { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual UserRole? Role { get; set; }
}
