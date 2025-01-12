using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Comment
{
    public int Id { get; set; }

    public string? Contents { get; set; }

    public DateTime? ModifyTime { get; set; }

    public int? Author { get; set; }

    public int? Post { get; set; }

    public int? Comment1 { get; set; }

    public string? Image { get; set; }

    public virtual User? AuthorNavigation { get; set; }

    public virtual Comment? Comment1Navigation { get; set; }

    public virtual ICollection<Comment> InverseComment1Navigation { get; set; } = new List<Comment>();

    public virtual Post? PostNavigation { get; set; }

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
}
