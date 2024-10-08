using System.Collections.Generic;
using FluentAssertions;
using Moq;
using Xunit;
using InternationalBusinessMen.Services;
using InternationalBusinessMenAPI.Domain;

namespace InternationalBusinessMen.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<IRateService> _mockRateService;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            // Configurar mock para el IRateService
            _mockRateService = new Mock<IRateService>();
            _mockRateService.Setup(service => service.ConvertToEUR(It.IsAny<decimal>(), It.IsAny<string>()))
                            .Returns<decimal, string>((amount, currency) => amount * 1.1m); // Ejemplo de conversión

            // Instanciar TransactionService con el mock
            _transactionService = new TransactionService(_mockRateService.Object);
        }

        [Fact]
        public void GetTransactionsBySku_ShouldReturnCorrectTransactions_WhenSkuExists()
        {
            // Arrange: Crear una lista de transacciones simulada
            var transactions = new List<Transaction>
            {
                new Transaction { Sku = "M20072", Amount = 20, Currency = "USD" },
                new Transaction { Sku = "M20072", Amount = 30, Currency = "EUR" },
                new Transaction { Sku = "M20073", Amount = 40, Currency = "JPY" }
            };

            // Establecer la lista simulada en el servicio de transacciones
            typeof(TransactionService)
                .GetField("_transactions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_transactionService, transactions);

            // Act: Llamar al método
            var result = _transactionService.GetTransactionsBySku("M20072");

            // Assert: Verificar que las transacciones obtenidas son las esperadas
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().ContainSingle(t => t.Sku == "M20072" && t.Amount == 22);
            result.Should().ContainSingle(t => t.Sku == "M20072" && t.Amount == 33);
        }

        [Fact]
        public void GetTotalAmountInEURBySku_ShouldReturnCorrectSumInEUR()
        {
            // Arrange: Crear una lista de transacciones simulada
            var transactions = new List<Transaction>
            {
                new Transaction { Sku = "M20072", Amount = 20, Currency = "USD" },
                new Transaction { Sku = "M20072", Amount = 30, Currency = "EUR" }
            };

            // Establecer la lista simulada en el servicio de transacciones
            typeof(TransactionService)
                .GetField("_transactions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_transactionService, transactions);

            // Act: Llamar al método para obtener el total en EUR
            var result = _transactionService.GetTotalAmountInEURBySku("M20072");

            // Assert: Verificar que el resultado es correcto
            result.Should().Be(55m); 
        }
    }
}
