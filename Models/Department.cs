using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class Department
{
    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
