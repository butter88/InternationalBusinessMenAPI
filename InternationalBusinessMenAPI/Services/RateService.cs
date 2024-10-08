using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;
using InternationalBusinessMenAPI.Domain;
using log4net;

namespace InternationalBusinessMen.Services
{
    public class RateService : IRateService
    {
        private readonly List<Rate> _rates;
        private static readonly ILog _log = LogManager.GetLogger(typeof(RateService));

        // Constructor: Carga las tasas desde el archivo JSON
        public RateService()
        {
            _log.Info("Iniciando carga de tasas desde el archivo JSON.");
            try
            {
                _rates = LoadRatesFromFile("Data/rates.json");
                _log.Info($"Se han cargado {_rates.Count} tasas desde el archivo.");
            }
            catch (FileNotFoundException ex)
            {
                _log.Error("Error: No se encontró el archivo de tasas.", ex);
                throw;
            }
            catch (Exception ex)
            {
                _log.Error("Error inesperado al cargar las tasas.", ex);
                throw;
            }
        }

        // Método privado para cargar las tasas desde el archivo JSON
        private List<Rate> LoadRatesFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                _log.Warn($"El archivo de tasas no se encuentra en la ruta: {filePath}");
                throw new FileNotFoundException($"El archivo de tasas no se encuentra: {filePath}");
            }

            try
            {
                _log.Info($"Leyendo tasas desde el archivo: {filePath}");
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<Rate>>(json);
            }
            catch (Exception ex)
            {
                _log.Error("Error al leer o deserializar el archivo de tasas.", ex);
                throw;
            }
        }

        // Obtiene la lista de todas las tasas cargadas
        public List<Rate> GetRates()
        {
            _log.Info("Solicitud para obtener todas las tasas de conversión.");
            return _rates;
        }

        // Método para convertir una cantidad de cualquier divisa a EUR
        public decimal ConvertToEUR(decimal amount, string currency)
        {
            _log.Info($"Iniciando conversión de {amount} {currency} a EUR.");
            if (currency == "EUR") return amount;

            try
            {
                var conversionRate = FindConversionRate(currency, "EUR");
                var convertedAmount = Math.Round(amount * conversionRate, 2, MidpointRounding.ToEven);
                _log.Info($"Conversión completada: {amount} {currency} son {convertedAmount} EUR.");
                return convertedAmount;
            }
            catch (Exception ex)
            {
                _log.Error($"Error al convertir {amount} {currency} a EUR.", ex);
                throw;
            }
        }

        // Encuentra la tasa de conversión directa o a través de una intermediaria
        private decimal FindConversionRate(string fromCurrency, string toCurrency)
        {
            _log.Info($"Buscando tasa de conversión desde {fromCurrency} a {toCurrency}.");
            var visited = new HashSet<string>();
            return FindConversionRateRecursive(fromCurrency, toCurrency, visited);
        }

        // Método recursivo para encontrar la tasa de conversión, incluso a través de intermediarios
        private decimal FindConversionRateRecursive(string fromCurrency, string toCurrency, HashSet<string> visited)
        {
            if (fromCurrency == toCurrency) return 1;

            // Intentar encontrar una tasa directa
            _log.Debug($"Buscando tasa directa de {fromCurrency} a {toCurrency}.");
            var directRate = _rates.FirstOrDefault(r => r.From == fromCurrency && r.To == toCurrency);
            if (directRate != null)
            {
                _log.Info($"Tasa directa encontrada: {fromCurrency} a {toCurrency} = {directRate.RateValue}");
                return directRate.RateValue;
            }

            // Intentar encontrar la tasa inversa si no se encuentra la directa
            _log.Debug($"Buscando tasa inversa de {toCurrency} a {fromCurrency}.");
            var inverseRate = _rates.FirstOrDefault(r => r.From == toCurrency && r.To == fromCurrency);
            if (inverseRate != null)
            {
                _log.Info($"Tasa inversa encontrada: {toCurrency} a {fromCurrency} = {1 / inverseRate.RateValue}");
                return 1 / inverseRate.RateValue;
            }

            // Añadir la divisa actual a las visitadas para evitar bucles
            visited.Add(fromCurrency);
            _log.Debug($"Añadiendo {fromCurrency} a la lista de visitadas.");

            // Intentar encontrar una tasa a través de intermediarios
            foreach (var rate in _rates.Where(r => r.From == fromCurrency && !visited.Contains(r.To)))
            {
                _log.Debug($"Buscando tasa intermedia desde {rate.From} a {rate.To}.");
                var intermediateRate = FindConversionRateRecursive(rate.To, toCurrency, visited);
                if (intermediateRate != 0)
                {
                    _log.Info($"Tasa intermedia encontrada para {fromCurrency} a {toCurrency} a través de {rate.To}. Tasa total: {rate.RateValue * intermediateRate}");
                    return rate.RateValue * intermediateRate;
                }
            }

            _log.Error($"No se encontró una tasa de conversión de {fromCurrency} a {toCurrency}.");
            throw new Exception($"No se encontró una tasa de conversión de {fromCurrency} a {toCurrency}");
        }
    }
}
