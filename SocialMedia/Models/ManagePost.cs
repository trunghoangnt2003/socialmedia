namespace SocialMedia.Models
{
    public class ManagePost
    {
        public int Id { get; set; }

        public string? Content { get; set; }

        public string? Author { get; set; }

        public int? ReactCount { get; set; }

        public int? CommentCount { get; set; }

        public DateTime? ModifyTime { get; set; }
    }
}
