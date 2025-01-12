using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class UserGroup
{
    public int Id { get; set; }

    public int? Group { get; set; }

    public int? User { get; set; }

    public DateTime? JoinTime { get; set; }

    public virtual Group? GroupNavigation { get; set; }

    public virtual User? UserNavigation { get; set; }
}
