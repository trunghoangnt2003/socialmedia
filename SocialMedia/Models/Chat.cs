using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Chat
{
    public int Id { get; set; }

    public string? Contents { get; set; }

    public int? Sender { get; set; }

    public int Receiver { get; set; }

    public int? Type { get; set; }

    public DateTime? SendTime { get; set; }

    public int? Status { get; set; }

    public virtual User ReceiverNavigation { get; set; } = null!;

    public virtual User? SenderNavigation { get; set; }
}
