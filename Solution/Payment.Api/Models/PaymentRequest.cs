namespace Payment.Api.Models;

public record PaymentRequest
{
    public string CardNumber { get; init; } = string.Empty;
    public string CardHolderName { get; init; } = string.Empty;
    public string ExpirationDate { get; init; } = string.Empty; // MM/YY
    public string Cvv { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "BRL";
    public string Description { get; init; } = string.Empty;
}
