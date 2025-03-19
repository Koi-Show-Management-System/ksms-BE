namespace KSMS.Domain.Dtos.Responses.RegistrationPayment;

public class RegistrationPaymentGetRegistrationResponse
{
    public Guid Id { get; set; }  
    public decimal PaidAmount { get; set; }
        
    public DateTime PaymentDate { get; set; }
    public string? QrcodeData { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }

    public string? Status { get; set; }
}