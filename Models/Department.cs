using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

[Index("DepartmentName", Name = "UQ__Departme__226ED1571B582033", IsUnique = true)]
public partial class Department
{
    [Key]
    [Column("department_id")]
    public int DepartmentId { get; set; }

    [Column("department_name")]
    [StringLength(100)]
    public string DepartmentName { get; set; } = null!;

    [Column("description")]
    public string? Description { get; set; }

    [InverseProperty("Department")]
    public virtual ICollection<TestAssignment> TestAssignments { get; set; } = new List<TestAssignment>();

    [InverseProperty("Department")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
