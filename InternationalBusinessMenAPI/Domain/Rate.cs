using Newtonsoft.Json;

namespace InternationalBusinessMenAPI.Domain
{
    public class Rate
    {
        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("rate")]
        public decimal RateValue { get; set; }  // Mantener RateValue pero mapear desde el JSON "rate"
    }
}
