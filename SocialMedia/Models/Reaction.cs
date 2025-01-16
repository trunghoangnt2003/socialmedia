using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Reaction
{
    public int Id { get; set; }

    public int? Post { get; set; }

    public int? Comment { get; set; }

    public int? Type { get; set; }

    public int? User { get; set; }

    public virtual Comment? CommentNavigation { get; set; }

    public virtual Post? PostNavigation { get; set; }

    public virtual User? UserNavigation { get; set; }
}
