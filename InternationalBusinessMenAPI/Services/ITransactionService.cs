using System.Collections.Generic;
using InternationalBusinessMenAPI.Domain;

namespace InternationalBusinessMen.Services
{
    public interface ITransactionService
    {
        List<Transaction> GetTransactions();
        List<Transaction> GetTransactionsBySku(string sku);
        decimal GetTotalAmountInEURBySku(string sku);
    }
}
