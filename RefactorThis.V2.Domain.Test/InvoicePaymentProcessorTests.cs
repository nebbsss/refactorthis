using Moq;
using RefactorThis.V2.Domain.Services;
using RefactorThis.V2.Persistence.Entities;
using RefactorThis.V2.Persistence.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading;

namespace RefactorThis.V2.Domain.Test
{
    public class InvoicePaymentProcessorTests
    {
        private readonly IRepository<InvoiceEntity> invoiceRepository = Mock.Of<IRepository<InvoiceEntity>>();
        private readonly IRepository<PaymentEntity> paymentRepository = Mock.Of<IRepository<PaymentEntity>>();
        private readonly InvoiceService systemUnderTest;

        public InvoicePaymentProcessorTests()
        {
            systemUnderTest = new(invoiceRepository, paymentRepository);
        }

       
        [Fact]
        public async void ProcessPayment_Should_ThrowException_When_NoInoiceFoundForPaymentReference()
        {

            InvoiceEntity? invoice = null;
            PaymentEntity payment = new PaymentEntity();
            Guid guid = Guid.NewGuid();
            string failureMessage = "";

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);

            try
            {
                var result = await systemUnderTest.ProcessPayment(guid, payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.Equal("There is no invoice matching this payment", failureMessage);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity();
            IEnumerable<PaymentEntity>? payments = null;
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 0,
                AmountPaid = 0
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("no payment needed", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity();
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Amount = 10
                }
            };
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 10,
                AmountPaid = 10
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("invoice was already fully paid", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity()
            {
                Amount = 6
            };
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Amount = 5
                }
            };
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 10,
                AmountPaid = 5
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("the payment is greater than the partial amount remaining", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity()
            {
                Amount = 6
            };
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>();
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 5,
                AmountPaid = 0
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("the payment is greater than the invoice amount", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity()
            {
                Amount = 5
            };
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Amount = 5
                }
            };
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 10,
                AmountPaid = 5
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("final partial payment received, invoice is now fully paid", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity()
            {
                Amount = 10
            };
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Amount = 10
                }
            };
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 10,
                AmountPaid = 0
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("invoice was already fully paid", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity()
            {
                Amount = 1
            };
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>
            {
                new PaymentEntity
                {
                    Amount = 5
                }
            };
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 10,
                AmountPaid = 5
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("another partial payment received, still not fully paid", result);
        }

        [Fact]
        public async void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            Guid guid = Guid.NewGuid();
            PaymentEntity payment = new PaymentEntity()
            {
                Amount = 1
            };
            IEnumerable<PaymentEntity>? payments = new List<PaymentEntity>();
            InvoiceEntity invoice = new InvoiceEntity()
            {
                InvoiceId = guid,
                Amount = 10,
                AmountPaid = 0
            };

            Mock.Get(invoiceRepository).Setup(v => v.GetById(guid, It.IsAny<CancellationToken>())).ReturnsAsync(invoice);
            Mock.Get(paymentRepository).Setup(v => v.GetFilteredAsync(It.IsAny<Expression<Func<PaymentEntity, bool>>>(), It.IsAny<CancellationToken>())).ReturnsAsync(payments);

            var result = await systemUnderTest.ProcessPayment(guid, payment);

            Assert.Equal("invoice is now partially paid", result);
        }
    }
}