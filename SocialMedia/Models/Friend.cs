using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Friend
{
    public int Id { get; set; }

    public int? User { get; set; }

    public int? Friend1 { get; set; }

    public int? Status { get; set; }

    public DateTime? SendTime { get; set; }

    public virtual User? Friend1Navigation { get; set; }

    public virtual User? UserNavigation { get; set; }
}
