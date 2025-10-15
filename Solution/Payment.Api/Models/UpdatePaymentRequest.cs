namespace Payment.Api.Models;

public record UpdatePaymentRequest
{
    public PaymentStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
}
