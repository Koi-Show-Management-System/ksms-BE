using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class RegistrationPayment
{
    public Guid Id { get; set; }

    public Guid RegistrationId { get; set; }

    public Guid PaymentTypeId { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual PaymentType PaymentType { get; set; } = null!;

    public virtual ICollection<Qrcode> Qrcodes { get; set; } = new List<Qrcode>();

    public virtual Registration Registration { get; set; } = null!;
}
