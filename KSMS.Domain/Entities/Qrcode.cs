using System;
using System.Collections.Generic;
using KSMS.Domain.Common;

namespace KSMS.Domain.Entities;

public partial class Qrcode : BaseEntity
{
    public Guid? TicketOrderDetailId { get; set; }

    public Guid? RegistrationPaymentId { get; set; }

    public string? QrcodeData { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual CheckInLog? CheckInLog { get; set; }

    public virtual RegistrationPayment? RegistrationPayment { get; set; }

    public virtual TicketOrderDetail? TicketOrderDetail { get; set; }
}
