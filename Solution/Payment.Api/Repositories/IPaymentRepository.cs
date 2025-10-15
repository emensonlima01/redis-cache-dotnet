using Payment.Api.Models;

namespace Payment.Api.Repositories;

public interface IPaymentRepository
{
    Task<PaymentResponse?> GetAsync(Guid transactionId);
    Task SetAsync(Guid transactionId, PaymentResponse payment);
    Task<bool> DeleteAsync(Guid transactionId);
}
