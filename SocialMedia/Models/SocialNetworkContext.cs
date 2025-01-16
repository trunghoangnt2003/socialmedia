using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SocialMedia.Models;

public partial class SocialNetworkContext : DbContext
{
    public SocialNetworkContext()
    {
    }

    public SocialNetworkContext(DbContextOptions<SocialNetworkContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Chat> Chats { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Friend> Friends { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<Reaction> Reactions { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserGroup> UserGroups { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(config.GetConnectionString("DBContext"));
        }
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chat>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Contents).HasColumnName("contents");
            entity.Property(e => e.Receiver).HasColumnName("receiver");
            entity.Property(e => e.SendTime)
                .HasColumnType("datetime")
                .HasColumnName("sendTime");
            entity.Property(e => e.Sender).HasColumnName("sender");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.ReceiverNavigation).WithMany(p => p.ChatReceiverNavigations)
                .HasForeignKey(d => d.Receiver)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Chats_Users1");

            entity.HasOne(d => d.SenderNavigation).WithMany(p => p.ChatSenderNavigations)
                .HasForeignKey(d => d.Sender)
                .HasConstraintName("FK_Chats_Users");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Comment1).HasColumnName("comment");
            entity.Property(e => e.Contents).HasColumnName("contents");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("modifyTime");
            entity.Property(e => e.Post).HasColumnName("post");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Author)
                .HasConstraintName("FK_Comments_Users");

            entity.HasOne(d => d.Comment1Navigation).WithMany(p => p.InverseComment1Navigation)
                .HasForeignKey(d => d.Comment1)
                .HasConstraintName("FK_Comments_Comments");

            entity.HasOne(d => d.PostNavigation).WithMany(p => p.Comments)
                .HasForeignKey(d => d.Post)
                .HasConstraintName("FK_Comments_Posts");
        });

        modelBuilder.Entity<Friend>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Friend1).HasColumnName("friend");
            entity.Property(e => e.SendTime)
                .HasColumnType("datetime")
                .HasColumnName("sendTime");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.Friend1Navigation).WithMany(p => p.FriendFriend1Navigations)
                .HasForeignKey(d => d.Friend1)
                .HasConstraintName("FK_Friends_Users1");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.FriendUserNavigations)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_Friends_Users");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Admin).HasColumnName("admin");
            entity.Property(e => e.Avatar).HasColumnName("avatar");
            entity.Property(e => e.CreateDate).HasColumnName("createDate");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Receiver).HasColumnName("receiver");
            entity.Property(e => e.SendTime)
                .HasColumnType("datetime")
                .HasColumnName("sendTime");
            entity.Property(e => e.Sender).HasColumnName("sender");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.ReceiverNavigation).WithMany(p => p.NotificationReceiverNavigations)
                .HasForeignKey(d => d.Receiver)
                .HasConstraintName("FK_Notifications_Users1");

            entity.HasOne(d => d.SenderNavigation).WithMany(p => p.NotificationSenderNavigations)
                .HasForeignKey(d => d.Sender)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author).HasColumnName("author");
            entity.Property(e => e.Contents).HasColumnName("contents");
            entity.Property(e => e.Group).HasColumnName("group");
            entity.Property(e => e.ModifyTime)
                .HasColumnType("datetime")
                .HasColumnName("modifyTime");
            entity.Property(e => e.Post1).HasColumnName("post");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.AuthorNavigation).WithMany(p => p.Posts)
                .HasForeignKey(d => d.Author)
                .HasConstraintName("FK_Posts_Users");

            entity.HasOne(d => d.Post1Navigation).WithMany(p => p.InversePost1Navigation)
                .HasForeignKey(d => d.Post1)
                .HasConstraintName("FK_Posts_Posts");
        });

        modelBuilder.Entity<Reaction>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.Post).HasColumnName("post");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.CommentNavigation).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.Comment)
                .HasConstraintName("FK_Reactions_Comments");

            entity.HasOne(d => d.PostNavigation).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.Post)
                .HasConstraintName("FK_Reactions_Posts");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.Reactions)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_Reactions_Users");
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Post).HasColumnName("post");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Url).HasColumnName("url");

            entity.HasOne(d => d.PostNavigation).WithMany(p => p.Resources)
                .HasForeignKey(d => d.Post)
                .HasConstraintName("FK_Resources_Posts");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Avatar).HasColumnName("avatar");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Online).HasColumnName("online");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("phone");
        });

        modelBuilder.Entity<UserGroup>(entity =>
        {
            entity.ToTable("UserGroup");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Group).HasColumnName("group");
            entity.Property(e => e.JoinTime)
                .HasColumnType("datetime")
                .HasColumnName("joinTime");
            entity.Property(e => e.User).HasColumnName("user");

            entity.HasOne(d => d.GroupNavigation).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.Group)
                .HasConstraintName("FK_UserGroup_Groups");

            entity.HasOne(d => d.UserNavigation).WithMany(p => p.UserGroups)
                .HasForeignKey(d => d.User)
                .HasConstraintName("FK_UserGroup_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
