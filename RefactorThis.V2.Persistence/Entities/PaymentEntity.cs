namespace RefactorThis.V2.Persistence.Entities;

public class PaymentEntity : IEntity
{
    public object? Id { get => this.PaymentId; set => this.PaymentId = value != null ? (Guid)value : Guid.Empty; }
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}
