using RefactorThis.V2.Persistence.Entities;

namespace RefactorThis.V2.Domain.Services;

public interface IInvoiceService
{
    Task<string?> ProcessPayment(Guid invoiceId, PaymentEntity payment);
}
