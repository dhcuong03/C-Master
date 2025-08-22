using System;
using System.Collections.Generic;

namespace TestMaster.Models;

public partial class Level
{
    public int LevelId { get; set; }

    public string LevelName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
