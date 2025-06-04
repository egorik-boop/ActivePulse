using System;
using System.Collections.Generic;

namespace ActivePulse.Entities;

public partial class UserRole
{
    public char RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
