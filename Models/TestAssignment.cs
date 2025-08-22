using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class TestAssignment
{
    public int AssignmentId { get; set; }

    public int TestId { get; set; }

    public int? UserId { get; set; }

    public int? DepartmentId { get; set; }

    public int? AssignedBy { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public virtual User? AssignedByNavigation { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Test Test { get; set; } = null!;

    public virtual User? User { get; set; }
}
