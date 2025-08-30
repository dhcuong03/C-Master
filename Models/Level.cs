using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TestMaster.Models;

[Index("LevelName", Name = "UQ__Levels__F94299E9ACA4381A", IsUnique = true)]
public partial class Level
{
    [Key]
    [Column("level_id")]
    public int LevelId { get; set; }

    [Column("level_name")]
    [StringLength(50)]
    public string LevelName { get; set; } = null!;

    [InverseProperty("Level")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
