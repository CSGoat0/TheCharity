using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.PaymentDTOs;
using TheCharityBLL.Services.Abstraction.Payment;

namespace TheCharityBLL.Services.Implementation.PaymentGateway
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymobService> _logger;
     
        private readonly IPaymentInfoService _paymentInfoService;

        public PaymobService( ILogger<PaymobService> logger, IPaymentInfoService paymentInfoService)
        {
            _httpClient = new HttpClient();
            _logger = logger;
            _paymentInfoService = paymentInfoService;
        }

        public async Task<ServiceResponse<string>> CreatePayment(decimal amount, string currency = "EGP")
        {
            string result= await CreatePayment(amount, metadata: null, billingData: null, currency);
            return new ServiceResponse<string>
            {
                Success = true,
                Data = result
                ,
                Message = "Payment created successfully"
            };
        }

        public async Task<string> CreatePayment(decimal amount, PaymentOrderMetadata? metadata, BillingData? billingData, string currency = "EGP")
        {
            // ── Step 1: Authentication ──────────────────────────────────────
            var _PaymentKeys = await _paymentInfoService.GetPaymentInfoByOrganizationIdAsync(metadata.OrganizationId);

            var authBody = JsonSerializer.Serialize(new { api_key = _PaymentKeys.Data.ApiKey });

            var authResponse = await _httpClient.PostAsync(
                "https://accept.paymob.com/api/auth/tokens",
                new StringContent(authBody, Encoding.UTF8, "application/json")
            );

            var authContent = await authResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Auth response: {Content}", authContent);

            if (!authResponse.IsSuccessStatusCode)
                throw new Exception($"Paymob auth failed ({authResponse.StatusCode}): {authContent}");

            var authJson = JsonDocument.Parse(authContent).RootElement;

            if (!authJson.TryGetProperty("token", out var authTokenEl))
                throw new Exception($"Paymob auth response missing 'token'. Response: {authContent}");

            var authToken = authTokenEl.GetString()!;

            var orderBody = JsonSerializer.Serialize(new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                currency,
                delivery_needed = false,
                items = Array.Empty<object>()

            });

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", authToken);



            var orderResponse = await _httpClient.PostAsync(
                "https://accept.paymob.com/api/ecommerce/orders",
                new StringContent(orderBody, Encoding.UTF8, "application/json")
            );

            var orderContent = await orderResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Order response: {Content}", orderContent);

            if (!orderResponse.IsSuccessStatusCode)
                throw new Exception($"Paymob order creation failed ({orderResponse.StatusCode}): {orderContent}");

            var orderJson = JsonDocument.Parse(orderContent).RootElement;

            if (!orderJson.TryGetProperty("id", out var orderIdEl))
                throw new Exception($"Paymob order response missing 'id'. Response: {orderContent}");

            var orderId = orderIdEl.GetInt64();

            var resolvedBilling = ResolveBillingData(billingData);



            // ── Step 3: Payment Key ─────────────────────────────────────────
            var paymentBody = JsonSerializer.Serialize(new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = orderId,
                currency,
                integration_id = int.Parse(_PaymentKeys.Data.IntegrationId),

                billing_data = resolvedBilling,
                extra = new Dictionary<string, string>
                {
                    ["user_id"] = metadata?.UserId ?? "",
                    ["campaign_id"] = metadata?.CampaignId.ToString() ?? ""
                }
            });

            var paymentResponse = await _httpClient.PostAsync(
                "https://accept.paymob.com/api/acceptance/payment_keys",
                new StringContent(paymentBody, Encoding.UTF8, "application/json")
            );

            var paymentContent = await paymentResponse.Content.ReadAsStringAsync();
            _logger.LogInformation("Payment key response: {Content}", paymentContent);

            if (!paymentResponse.IsSuccessStatusCode)
                throw new Exception($"Paymob payment key failed ({paymentResponse.StatusCode}): {paymentContent}");

            var paymentJson = JsonDocument.Parse(paymentContent).RootElement;

            if (!paymentJson.TryGetProperty("token", out var paymentTokenEl))
                throw new Exception($"Paymob payment key response missing 'token'. Response: {paymentContent}");

            var paymentKey = paymentTokenEl.GetString()!;

            // ── Step 4: Return iFrame URL ───────────────────────────────────
            return $"https://accept.paymob.com/api/acceptance/iframes/{_PaymentKeys.Data.IframeId}?payment_token={paymentKey}";
        }
        private static object ResolveBillingData(BillingData? billing)
        {
            if (billing is null)
            {
                return new
                {
                    first_name = "NA",
                    last_name = "NA",
                    email = "NA",
                    phone_number = "NA",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "EG",
                    state = "NA"
                };
            }

            return new
            {
                first_name = billing.FirstName ?? "NA",
                last_name = billing.LastName ?? "NA",
                email = billing.Email ?? "NA",
                phone_number = billing.PhoneNumber ?? "NA",
                apartment = billing.Apartment ?? "NA",
                floor = billing.Floor ?? "NA",
                street = billing.Street ?? "NA",
                building = billing.Building ?? "NA",
                shipping_method = "NA",
                postal_code = billing.PostalCode ?? "NA",
                city = billing.City ?? "NA",
                country = billing.Country ?? "EG",
                state = billing.State ?? "NA"
            };
        }

       
    }
}