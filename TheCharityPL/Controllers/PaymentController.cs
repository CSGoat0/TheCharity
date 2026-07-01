using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaymentDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Abstraction.MoneyDonation;
using TheCharityBLL.Services.Abstraction.Payment;
using TheCharityBLL.Services.Repository;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymobService _paymobService;
        private readonly IDonationService _donationService;
        private readonly ILogger<PaymentController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public PaymentController(
            IPaymobService paymobService,
            IDonationService donationService,
            ILogger<PaymentController> logger,
            IConfiguration configuration,
            IUserService userService)
        {
            _paymobService = paymobService;
            _donationService = donationService;
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
        }
        /// <summary>
        /// create payment request to donate to specific campaign by user
        /// </summary>
        [HttpPost("create")]
              [Authorize]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identity could not be resolved." });

            //  Fetch user via repository to build real billing data
            var user = await _userService.GetUserByIdAsync(userId);
            if (user is null)
                return Unauthorized(new { message = "User not found." });

            //  Split FullName → FirstName / LastName (Paymob requires them separately)
            var nameParts = (user.FullName ?? "NA").Split(' ', 2);
            var billing = new BillingData
            {
                FirstName = nameParts[0],
                LastName = nameParts.Length > 1 ? nameParts[1] : "NA",
                Email = user.Email ?? "NA",
                PhoneNumber = user.PhoneNumber ?? "NA",
                Street = user.Address ?? "NA",
                Country = "EG"
            };

            var metadata = new PaymentOrderMetadata
            {
                UserId = userId,
                CampaignId = request.CampaignId,
                OrganizationId = request.OrganizationId
            };

            var iframeUrl = await _paymobService.CreatePayment(request.Amount, metadata, billing);

            _logger.LogInformation(
                "Payment session created. UserId: {UserId}, CampaignId: {CampaignId}",
                userId, request.CampaignId);

            return Ok(new { iframeUrl });
        }

        // =====================================================================
        // POST api/payment/callback
        /// <summary>
        /// Anonymous — called by Paymob after payment.
        /// UserId + CampaignId are read from order metadata (no server state needed).
        /// On success: creates the donation record.
        /// Always returns 200 — Paymob retries on any other status code.
        /// </summary>
        // =====================================================================
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromBody] PaymobCallbackWrapper wrapper)
        {
            try
            {
                _logger.LogInformation("Paymob callback received. Type: {Type}", wrapper?.Type);

                if (wrapper?.Obj is null)
                {
                    _logger.LogWarning("Paymob callback wrapper or obj is null.");
                    return BadRequest(new { message = "Invalid callback data." });
                }

                var transaction = wrapper.Obj;

                // 1. Verify HMAC signature
                var receivedHmac = Request.Query["hmac"].ToString();
                if (!VerifyHmac(transaction, receivedHmac))
                {
                    _logger.LogWarning("Invalid HMAC for transaction {TransactionId}.", transaction.Id);
                    return Unauthorized(new { message = "Invalid HMAC signature." });
                }

                // 2. Check transaction outcome
                if (!transaction.Success)
                {
                    _logger.LogInformation(
                        "Payment not successful. OrderId: {OrderId}, TransactionId: {TransactionId}.",
                        transaction.OrderId, transaction.Id);

                    return Ok(new { message = "Payment not successful.", status = "failed" });
                }

                // 3. Extract UserId + CampaignId from Paymob order metadata

                var userId = transaction.PaymentKeyClaims?.Extra?["user_id"]?.ToString();
                var campaignIdRaw = transaction.PaymentKeyClaims?.Extra?["campaign_id"]?.ToString();
                var campaignId = int.TryParse(campaignIdRaw, out var cid) ? cid : (int?)null;

                if (
                     string.IsNullOrEmpty(userId)
                    || campaignId == 0)
                {

                    _logger.LogError(
                        "Missing or incomplete metadata on callback. OrderId: {OrderId}.",
                        transaction.OrderId);

                    return Ok(new { message = "Callback received but donation could not be recorded: missing metadata.", status = "error" });
                }

                // 4. Create donation record
                var donationDto = new CreateDonationDto
                {
                    Amount = (double)(transaction.AmountCents / 100m),
                    UserId = userId,
                    CampaignId = campaignId
                };

                var donation = await _donationService.CreateDonationAsync(donationDto);

                _logger.LogInformation(
                    "Donation created. DonationId: {DonationId}, OrderId: {OrderId}, " +
                    "TransactionId: {TransactionId}, Amount: {Amount} {Currency}, " +
                    "UserId: {UserId}, CampaignId: {CampaignId}.",
                    donation.Id, transaction.OrderId, transaction.Id,
                    donationDto.Amount, transaction.Currency ?? "EGP",
                    userId, campaignId);

                // 5. Always return 200 to Paymob
                return Ok(new
                {
                    message = "Callback processed successfully.",
                    transaction_id = transaction.Id,
                    order_id = transaction.OrderId,
                    donation_id = donation.Id,
                    status = "success"
                });
            }
            catch (InvalidOperationException ex)
            {
                // Thrown by DonationService when IsDonationValidAsync returns false
                _logger.LogWarning(ex, "Donation validation failed during callback.");
                return Ok(new { message = "Payment received but donation validation failed.", status = "error" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing Paymob callback.");
                return Ok(new { message = "Callback received but processing failed.", status = "error" });
            }
        }

        private bool VerifyHmac(PaymobTransaction transaction, string receivedHmac)
        {
            var secret = _configuration["Paymob:HmacKey"];
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(receivedHmac))
            {
                _logger.LogWarning("HMAC verification failed: missing secret or received HMAC");
                return false;
            }

            try
            {
                // Build the string according to Paymob's specification
                var data = string.Concat(
                    transaction.AmountCents,
                    transaction.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    transaction.Currency,
                    transaction.ErrorOccured.ToString().ToLowerInvariant(),
                    transaction.HasParentTransaction.ToString().ToLowerInvariant(),
                    transaction.Id,
                    transaction.IntegrationId,
                    transaction.Is3dSecure.ToString().ToLowerInvariant(),
                    transaction.IsAuth.ToString().ToLowerInvariant(),
                    transaction.IsCapture.ToString().ToLowerInvariant(),
                    transaction.IsRefunded.ToString().ToLowerInvariant(),
                    transaction.IsStandalonePayment.ToString().ToLowerInvariant(),
                    transaction.IsVoided.ToString().ToLowerInvariant(),
                    transaction.Order?.Id ?? 0,
                    transaction.Owner,
                    transaction.Pending.ToString().ToLowerInvariant(),
                    transaction.SourceData?.Pan ?? "",
                    transaction.SourceData?.SubType ?? "",
                    transaction.SourceData?.Type ?? "",
                    transaction.Success.ToString().ToLowerInvariant()
                );

                _logger.LogDebug("HMAC data string: {Data}", data);

                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(secret));
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                var computed = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

                var isValid = computed == receivedHmac.ToLowerInvariant();

                if (!isValid)
                {
                    _logger.LogWarning("HMAC mismatch. Computed: {Computed}, Received: {Received}",
                        computed, receivedHmac.ToLowerInvariant());
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing HMAC");
                return false;
            }
        }

    }
}
