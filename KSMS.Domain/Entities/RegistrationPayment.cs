using System;
using System.Collections.Generic;

namespace KSMS.Domain.Entities;

public partial class RegistrationPayment
{
    public Guid Id { get; set; }

    public Guid RegistrationId { get; set; }

    public string? QrcodeData { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string TransactionCode { get; set; } = null!;

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;
    
    public string? PaymentUrl { get; set; }

    public virtual Registration Registration { get; set; } = null!;
}
