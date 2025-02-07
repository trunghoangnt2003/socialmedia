using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Password { get; set; }

    public string? Avatar { get; set; }

    public bool IsActive { get; set; }

    public bool? Online { get; set; }
    public bool? Gender { get; set; }

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
