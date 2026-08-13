using System.Text.Json.Serialization;

namespace RailLinkBackEnd.Entity
{
    public class TransportRequest
    {
        [JsonPropertyName("origin")]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("destination")]
        public string Destination { get; set; } = string.Empty;

        [JsonPropertyName("cargo_weight_ton")]
        public int CargoWeightTon { get; set; }

        [JsonPropertyName("shipping_date")]
        public string ShippingDate { get; set; } = string.Empty; 

        [JsonPropertyName("priority")]
        public string Priority { get; set; } = string.Empty;
    }
}
