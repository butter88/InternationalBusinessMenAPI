using Microsoft.AspNetCore.Mvc;
using InternationalBusinessMen.Services;
using log4net;

namespace InternationalBusinessMen.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IRateService _rateService;
        private static readonly ILog _log = LogManager.GetLogger(typeof(TransactionsController));

        // Constructor del controlador que utiliza la inyección de dependencias para los servicios
        public TransactionsController(ITransactionService transactionService, IRateService rateService)
        {
            _transactionService = transactionService;
            _rateService = rateService;
        }

        // Endpoint para obtener todas las transacciones disponibles
        [HttpGet]
        public IActionResult GetAllTransactions()
        {
            _log.Info("Solicitud para obtener todas las transacciones.");
            try
            {
                var transactions = _transactionService.GetTransactions();
                _log.Info($"Se obtuvieron {transactions.Count} transacciones.");
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _log.Error("Error al obtener todas las transacciones.", ex);
                return StatusCode(500, "Ocurrió un error al obtener las transacciones.");
            }
        }

        // Endpoint para obtener todas las tasas de conversión disponibles
        [HttpGet("rates")]
        public IActionResult GetRates()
        {
            _log.Info("Solicitud para obtener todas las tasas de conversión.");
            try
            {
                var rates = _rateService.GetRates();
                _log.Info($"Se obtuvieron {rates.Count} tasas de conversión.");
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _log.Error("Error al obtener las tasas de conversión.", ex);
                return StatusCode(500, "Ocurrió un error al obtener las tasas de conversión.");
            }
        }

        // Endpoint para obtener todas las transacciones de un SKU específico y la suma total en EUR
        [HttpGet("{sku}")]
        public IActionResult GetTransactionsBySku(string sku)
        {
            _log.Info($"Solicitud para obtener transacciones para el SKU: {sku}");
            try
            {
                var transactions = _transactionService.GetTransactionsBySku(sku);
                if (transactions == null || transactions.Count == 0)
                {
                    _log.Warn($"No se encontraron transacciones para el SKU: {sku}");
                    return NotFound($"No se encontraron transacciones para el SKU: {sku}");
                }

                var totalAmountInEUR = _transactionService.GetTotalAmountInEURBySku(sku);
                _log.Info($"Se obtuvieron {transactions.Count} transacciones para el SKU: {sku}. Total en EUR: {totalAmountInEUR}");

                return Ok(new
                {
                    Sku = sku,
                    Transactions = transactions,
                    TotalAmountInEUR = totalAmountInEUR
                });
            }
            catch (Exception ex)
            {
                _log.Error($"Error al obtener transacciones para el SKU: {sku}", ex);
                return StatusCode(500, $"Ocurrió un error al obtener transacciones para el SKU: {sku}");
            }
        }
    }
}
