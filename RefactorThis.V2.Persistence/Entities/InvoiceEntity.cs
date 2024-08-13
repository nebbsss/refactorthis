using RefactorThis.V2.Persistence.Enumerations;

namespace RefactorThis.V2.Persistence.Entities;

public class InvoiceEntity : IEntity
{
    public object? Id { get => this.InvoiceId; set => this.InvoiceId = value != null ? (Guid)value : Guid.Empty; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal TaxAmount { get; set; }
    public InvoiceType Type { get; set; }
    public List<PaymentEntity>? Payments { get; set; }
}
