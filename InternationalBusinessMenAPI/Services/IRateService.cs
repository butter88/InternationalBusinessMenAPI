namespace InternationalBusinessMen.Services
{
    using InternationalBusinessMenAPI.Domain;
    using System.Collections.Generic;

    public interface IRateService
    {
        List<Rate> GetRates();
        decimal ConvertToEUR(decimal amount, string currency);
    }
}