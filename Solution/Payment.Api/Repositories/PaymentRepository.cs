using Payment.Api.Models;
using Payment.Api.Repositories.Base;

namespace Payment.Api.Repositories;

public class PaymentRepository(IRedisCache cache) : IPaymentRepository
{
    private const string KeyPrefix = "payment:";
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(30);

    public async Task<PaymentResponse?> GetAsync(Guid transactionId)
    {
        var key = GetKey(transactionId);
        return await cache.GetAsync<PaymentResponse>(key);
    }

    public async Task SetAsync(Guid transactionId, PaymentResponse payment)
    {
        var key = GetKey(transactionId);
        await cache.SetAsync(key, payment, DefaultExpiration);
    }

    public async Task<bool> DeleteAsync(Guid transactionId)
    {
        var key = GetKey(transactionId);
        return await cache.DeleteAsync(key);
    }

    private static string GetKey(Guid transactionId) => $"{KeyPrefix}{transactionId}";
}
