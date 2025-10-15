namespace Payment.Api.Models;

public record PaymentResponse
{
    public Guid TransactionId { get; init; }
    public string CardNumberMasked { get; init; } = string.Empty;
    public string CardHolderName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public PaymentStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
