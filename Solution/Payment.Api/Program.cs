using Payment.Api.Models;
using Payment.Api.Repositories;
using Payment.Api.Repositories.Base;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnectionString);

builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddSingleton<IRedisCache, RedisCache>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost("/payments", async (PaymentRequest request, IPaymentRepository repository) =>
{
    var transactionId = Guid.NewGuid();

    var payment = new PaymentResponse
    {
        TransactionId = transactionId,
        CardNumberMasked = MaskCardNumber(request.CardNumber),
        CardHolderName = request.CardHolderName,
        Amount = request.Amount,
        Currency = request.Currency,
        Description = request.Description,
        Status = PaymentStatus.Pending,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = null
    };

    await repository.SetAsync(transactionId, payment);

    return Results.Created($"/payments/{transactionId}", payment);
})
.WithName("CreatePayment");

app.MapGet("/payments/{id:guid}", async (Guid id, IPaymentRepository repository) =>
{
    var payment = await repository.GetAsync(id);

    if (payment == null)
    {
        return Results.NotFound(new { Message = "Pagamento não encontrado" });
    }

    return Results.Ok(payment);
})
.WithName("GetPaymentById");

app.MapPut("/payments/{id:guid}", async (Guid id, UpdatePaymentRequest request, IPaymentRepository repository) =>
{
    var existingPayment = await repository.GetAsync(id);

    if (existingPayment == null)
    {
        return Results.NotFound(new { Message = "Pagamento não encontrado" });
    }

    var updatedPayment = existingPayment with
    {
        Status = request.Status,
        Description = string.IsNullOrWhiteSpace(request.Description)
            ? existingPayment.Description
            : request.Description,
        UpdatedAt = DateTime.UtcNow
    };

    await repository.SetAsync(id, updatedPayment);

    return Results.Ok(updatedPayment);
})
.WithName("UpdatePayment");

app.MapDelete("/payments/{id:guid}", async (Guid id, IPaymentRepository repository) =>
{
    var deleted = await repository.DeleteAsync(id);

    if (!deleted)
    {
        return Results.NotFound(new { Message = "Pagamento não encontrado" });
    }

    return Results.NoContent();
})
.WithName("DeletePayment");

app.Run();

static string MaskCardNumber(string cardNumber)
{
    if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
    {
        return "****";
    }

    var lastFour = cardNumber[^4..];
    return $"**** **** **** {lastFour}";
}