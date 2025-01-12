using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Resource
{
    public int Id { get; set; }

    public string? Url { get; set; }

    public int? Post { get; set; }

    public int? Type { get; set; }

    public virtual Post? PostNavigation { get; set; }
}
