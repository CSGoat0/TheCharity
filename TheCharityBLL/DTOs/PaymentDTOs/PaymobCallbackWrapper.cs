using System.Text.Json.Serialization;

namespace TheCharityBLL.DTOs.PaymentDTOs
{
    public class PaymobCallbackWrapper
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("obj")]
        public PaymobTransaction? Obj { get; set; }

        [JsonPropertyName("accept_fees")]
        public decimal AcceptFees { get; set; }

        [JsonPropertyName("hmac")]
        public string? Hmac { get; set; }

        [JsonPropertyName("issuer_bank")]
        public object? IssuerBank { get; set; }

        [JsonPropertyName("transaction_processed_callback_responses")]
        public object? TransactionProcessedCallbackResponses { get; set; } // Can be string or array
    }
}