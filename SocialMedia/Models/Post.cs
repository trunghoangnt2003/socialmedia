using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Post
{
    public int Id { get; set; }

    public string? Contents { get; set; }

    public DateTime? ModifyTime { get; set; }

    public int? Author { get; set; }

    public int? Type { get; set; }

    public int? Post1 { get; set; }

    public int? Group { get; set; }

    public virtual User? AuthorNavigation { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Post> InversePost1Navigation { get; set; } = new List<Post>();

    public virtual Post? Post1Navigation { get; set; }

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
