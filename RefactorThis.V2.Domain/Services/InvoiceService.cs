using RefactorThis.V2.Persistence.Entities;
using RefactorThis.V2.Persistence.Enumerations;
using RefactorThis.V2.Persistence.Repositories;

namespace RefactorThis.V2.Domain.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<InvoiceEntity> _invoiceRepository;
    private readonly IRepository<PaymentEntity> _paymentRepository;
    public InvoiceService(IRepository<InvoiceEntity> invoiceRepository, IRepository<PaymentEntity> paymentRepository)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoiceRepository));
        ArgumentNullException.ThrowIfNull(nameof(paymentRepository));

        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<string?> ProcessPayment(Guid invoiceId, PaymentEntity payment)
    {
        var invoice = await _invoiceRepository.GetById(invoiceId);
        if(invoice is null) throw new InvalidOperationException("There is no invoice matching this payment");

        var payments = await _paymentRepository.GetFilteredAsync(p => p.InvoiceId == invoiceId);
        if(invoice.Amount == 0)
        {
            if (payments is null || !payments.Any())
            {
                return "no payment needed";
            }

            throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
        }

        bool hasPaymentRecords = payments is not null && payments.Any();
        decimal paymentSum = hasPaymentRecords ? payments!.Sum(p => p.Amount) : 0;

        if (paymentSum != 0)
        {
            if (invoice.Amount == paymentSum)
            {
                return "invoice was already fully paid";
            }
            else if (payment.Amount > (invoice.Amount - invoice.AmountPaid))
            {
                return "the payment is greater than the partial amount remaining";
            }
        }
        else if (payment.Amount > invoice.Amount)
        {
            return "the payment is greater than the invoice amount";
        }

        if (hasPaymentRecords)
        {
            decimal amountPaid = invoice.AmountPaid;
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.AmountPaid += payment.Amount;
                    break;
                case InvoiceType.Commercial:
                    invoice.AmountPaid += payment.Amount;
                    invoice.TaxAmount += payment.Amount * 0.14m;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _paymentRepository.Create(payment);
            await _invoiceRepository.Update(invoice);

            if ((invoice.Amount - amountPaid) == payment.Amount)
            {
                return "final partial payment received, invoice is now fully paid";
            }
            else
            {
                return "another partial payment received, still not fully paid";
            }
        }
        else
        {
            switch (invoice.Type)
            {
                case InvoiceType.Standard:
                    invoice.AmountPaid = payment.Amount;
                    invoice.TaxAmount = payment.Amount * 0.14m;
                    break;
                case InvoiceType.Commercial:
                    invoice.AmountPaid = payment.Amount;
                    invoice.TaxAmount = payment.Amount * 0.14m;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await _paymentRepository.Create(payment);
            await _invoiceRepository.Update(invoice);

            if (invoice.Amount == payment.Amount)
            {
                return "invoice is now fully paid";
            }
            else
            {
                return "invoice is now partially paid";
            }
        }
    }
}
