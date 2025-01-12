using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Group
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateOnly? CreateDate { get; set; }

    public int? Admin { get; set; }

    public string? Avatar { get; set; }

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
