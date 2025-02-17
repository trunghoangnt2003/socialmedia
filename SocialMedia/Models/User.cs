using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SocialMedia.Models;

public partial class User
{
    public int Id { get; set; }

    [Display(Name = "Full Name")]
    public string? Name { get; set; }

    [Display(Name = "Date of Birth")]
    //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateOnly? Dob { get; set; }

    [Display(Name = "Phone Number")]
    public string? Phone { get; set; }

    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Display(Name = "Residential Address")]
    public string? Address { get; set; }

    public string? Password { get; set; }

    public string? Avatar { get; set; }

    [Display(Name = "Active Status")]
    public bool IsActive { get; set; }

    public bool? Online { get; set; }
    public bool? Gender { get; set; }

    public int Role { get; set; }

    public virtual ICollection<Chat> ChatReceiverNavigations { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatSenderNavigations { get; set; } = new List<Chat>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Friend> FriendFriend1Navigations { get; set; } = new List<Friend>();

    public virtual ICollection<Friend> FriendUserNavigations { get; set; } = new List<Friend>();

    public virtual ICollection<Notification> NotificationReceiverNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationSenderNavigations { get; set; } = new List<Notification>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

    public virtual ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
}
