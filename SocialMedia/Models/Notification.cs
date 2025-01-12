using System;
using System.Collections.Generic;

namespace SocialMedia.Models;

public partial class Notification
{
    public int Id { get; set; }

    public int? Sender { get; set; }

    public int? Receiver { get; set; }

    public DateTime? SendTime { get; set; }

    public string? Message { get; set; }

    public int? Status { get; set; }

    public virtual User? ReceiverNavigation { get; set; }

    public virtual User? SenderNavigation { get; set; }
}
