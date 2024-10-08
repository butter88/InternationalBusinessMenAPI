using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using InternationalBusinessMenAPI.Domain;
using log4net;

namespace InternationalBusinessMen.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly List<Transaction> _transactions; // Lista de transacciones cargadas desde el archivo JSON
        private readonly IRateService _rateService; // Servicio de conversión de tasas
        private static readonly ILog _log = LogManager.GetLogger(typeof(TransactionService));

        // Constructor: inicializa el servicio de transacciones y carga las transacciones
        public TransactionService(IRateService rateService)
        {
            _log.Info("Iniciando carga de transacciones desde el archivo JSON.");
            _rateService = rateService;
            try
            {
                _transactions = LoadTransactionsFromFile("Data/transactions.json");
                _log.Info($"Se han cargado {_transactions.Count} transacciones desde el archivo.");
            }
            catch (FileNotFoundException ex)
            {
                _log.Error("Error: No se encontró el archivo de transacciones.", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Error inesperado al cargar las transacciones.", ex);
                throw;
            }
        }

        // Método privado para cargar las transacciones desde un archivo JSON
        private List<Transaction> LoadTransactionsFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _log.Warn($"El archivo de transacciones no se encuentra en la ruta: {filePath}");
                throw new FileNotFoundException($"El archivo de transacciones no se encuentra: {filePath}");
            }

            try
            {
                _log.Info($"Leyendo transacciones desde el archivo: {filePath}");
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Transaction>>(json);
            }
            catch (Exception ex)
            {
                _log.Error("Error al leer o deserializar el archivo de transacciones.", ex);
                throw;
            }
        }

        // Devuelve la lista completa de transacciones
        public List<Transaction> GetTransactions()
        {
            _log.Info("Solicitud para obtener todas las transacciones.");
            return _transactions;
        }

        // Obtiene todas las transacciones para un SKU específico, convertidas a EUR
        public List<Transaction> GetTransactionsBySku(string sku)
        {
            _log.Info($"Solicitud para obtener transacciones para el SKU: {sku}");
            var transactionsInEUR = GetTransactionsInEURBySku(sku);
            if (transactionsInEUR == null || !transactionsInEUR.Any())
            {
                _log.Warn($"No se encontraron transacciones para el SKU: {sku}");
            }
            else
            {
                _log.Info($"Se encontraron {transactionsInEUR.Count} transacciones para el SKU: {sku}");
            }
            return transactionsInEUR;
        }

        // Obtiene una lista de transacciones para un SKU específico, pero convertidas a EUR
        public List<Transaction> GetTransactionsInEURBySku(string sku)
        {
            _log.Info($"Iniciando la conversión de transacciones a EUR para el SKU: {sku}");

            // Filtra las transacciones con el SKU proporcionado
            var transactions = _transactions.Where(t => t.Sku == sku).ToList();
            if (transactions == null || !transactions.Any())
            {
                _log.Warn($"No se encontraron transacciones para el SKU: {sku} antes de la conversión.");
            }

            // Convierte cada transacción a EUR usando el servicio de tasas y redondea la cantidad a dos decimales
            var transactionsInEUR = transactions.Select(t =>
            {
                try
                {
                    var convertedAmount = Math.Round(_rateService.ConvertToEUR(t.Amount, t.Currency), 2, MidpointRounding.ToEven);
                    _log.Info($"Transacción convertida: {t.Amount} {t.Currency} -> {convertedAmount} EUR para SKU: {t.Sku}");
                    return new Transaction
                    {
                        Sku = t.Sku,
                        Amount = convertedAmount,
                        Currency = "EUR"
                    };
                }
                catch (Exception ex)
                {
                    _log.Error($"Error al convertir la transacción {t.Amount} {t.Currency} para el SKU: {t.Sku} a EUR.", ex);
                    throw;
                }
            }).ToList();

            return transactionsInEUR;
        }

        // Calcula la suma total de todas las transacciones para un SKU específico, en EUR
        public decimal GetTotalAmountInEURBySku(string sku)
        {
            _log.Info($"Calculando el monto total en EUR para el SKU: {sku}");

            try
            {
                // Obtiene todas las transacciones del SKU específico, convertidas a EUR
                var transactionsInEUR = GetTransactionsInEURBySku(sku);
                var totalAmount = Math.Round(transactionsInEUR.Sum(t => t.Amount), 2, MidpointRounding.ToEven);
                _log.Info($"Monto total en EUR para el SKU: {sku} es {totalAmount}");
                return totalAmount;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al calcular el monto total en EUR para el SKU: {sku}", ex);
                throw;
            }
        }
    }
}
