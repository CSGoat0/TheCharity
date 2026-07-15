using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.DTOs.PaymentDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Abstraction.MoneyDonation;
using TheCharityBLL.Services.Abstraction.Payment;

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
            _paymobService = paymobService ?? throw new ArgumentNullException(nameof(paymobService));
            _donationService = donationService ?? throw new ArgumentNullException(nameof(donationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        // ==============================
        // POST: api/payment/create
        // ==============================

        /// <summary>
        /// Create payment request to donate to specific campaign by user
        /// </summary>
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreatePaymentRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "Invalid payment request."
                });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ServiceResponse<object?>
                {
                    Success = false,
                    Message = "User identity could not be resolved."
                });
            }

            try
            {
                _logger.LogInformation("Creating payment for user: {UserId}, CampaignId: {CampaignId}", userId, request.CampaignId);

                // Fetch user via repository to build real billing data
                var userResult = await _userService.GetUserByIdAsync(userId);
                if (!userResult.Success || userResult.Data == null)
                {
                    return Unauthorized(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = "User not found."
                    });
                }

                var user = userResult.Data;

                // Split FullName → FirstName / LastName (Paymob requires them separately)
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

                return Ok(new ServiceResponse<string>
                {
                    Success = true,
                    Data = iframeUrl,
                    Message = "Payment session created successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for user: {UserId}", userId);
                return StatusCode(500, new ServiceResponse<object?>
                {
                    Success = false,
                    Message = $"An error occurred while creating the payment: {ex.Message}"
                });
            }
        }

        // ==============================
        // POST: api/payment/callback
        // ==============================

        /// <summary>
        /// Anonymous — called by Paymob after payment.
        /// UserId + CampaignId are read from order metadata (no server state needed).
        /// On success: creates the donation record.
        /// Always returns 200 — Paymob retries on any other status code.
        /// </summary>
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
                    return BadRequest(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = "Invalid callback data."
                    });
                }

                var transaction = wrapper.Obj;

                // 1. Verify HMAC signature
                var receivedHmac = Request.Query["hmac"].ToString();
                if (!VerifyHmac(transaction, receivedHmac))
                {
                    _logger.LogWarning("Invalid HMAC for transaction {TransactionId}.", transaction.Id);
                    return Unauthorized(new ServiceResponse<object?>
                    {
                        Success = false,
                        Message = "Invalid HMAC signature."
                    });
                }

                // 2. Check transaction outcome
                if (!transaction.Success)
                {
                    _logger.LogInformation(
                        "Payment not successful. OrderId: {OrderId}, TransactionId: {TransactionId}.",
                        transaction.OrderId, transaction.Id);

                    return Ok(new ServiceResponse<object?>
                    {
                        Success = true,
                        Message = "Payment not successful.",
                        Data = new { status = "failed" }
                    });
                }

                // 3. Extract UserId + CampaignId from Paymob order metadata
                var userId = transaction.PaymentKeyClaims?.Extra?["user_id"]?.ToString();
                var campaignIdRaw = transaction.PaymentKeyClaims?.Extra?["campaign_id"]?.ToString();
                var campaignId = int.TryParse(campaignIdRaw, out var cid) ? cid : (int?)null;

                if (string.IsNullOrEmpty(userId) || campaignId == null || campaignId == 0)
                {
                    _logger.LogError(
                        "Missing or incomplete metadata on callback. OrderId: {OrderId}.",
                        transaction.OrderId);

                    return Ok(new ServiceResponse<object?>
                    {
                        Success = true,
                        Message = "Callback received but donation could not be recorded: missing metadata.",
                        Data = new { status = "error" }
                    });
                }

                // 4. Create donation record
                var donationDto = new CreateDonationDto
                {
                    Amount = (double)(transaction.AmountCents / 100m),
                    UserId = userId,
                    CampaignId = campaignId
                };

                var donationResult = await _donationService.CreateDonationAsync(donationDto);

                if (!donationResult.Success)
                {
                    _logger.LogWarning(
                        "Donation creation failed. OrderId: {OrderId}, Error: {Error}",
                        transaction.OrderId, donationResult.Message);

                    return Ok(new ServiceResponse<object?>
                    {
                        Success = true,
                        Message = "Callback received but donation creation failed.",
                        Data = new { status = "error", error = donationResult.Message }
                    });
                }

                _logger.LogInformation(
                    "Donation created. DonationId: {DonationId}, OrderId: {OrderId}, " +
                    "TransactionId: {TransactionId}, Amount: {Amount} {Currency}, " +
                    "UserId: {UserId}, CampaignId: {CampaignId}.",
                    donationResult.Data?.Id, transaction.OrderId, transaction.Id,
                    donationDto.Amount, transaction.Currency ?? "EGP",
                    userId, campaignId);

                // 5. Always return 200 to Paymob
                return Ok(new ServiceResponse<object?>
                {
                    Success = true,
                    Message = "Callback processed successfully.",
                    Data = new
                    {
                        transaction_id = transaction.Id,
                        order_id = transaction.OrderId,
                        donation_id = donationResult.Data?.Id,
                        status = "success"
                    }
                });
            }
            catch (InvalidOperationException ex)
            {
                // Thrown by DonationService when IsDonationValidAsync returns false
                _logger.LogWarning(ex, "Donation validation failed during callback.");
                return Ok(new ServiceResponse<object?>
                {
                    Success = true,
                    Message = "Payment received but donation validation failed.",
                    Data = new { status = "error" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing Paymob callback.");
                return Ok(new ServiceResponse<object?>
                {
                    Success = true,
                    Message = "Callback received but processing failed.",
                    Data = new { status = "error" }
                });
            }
        }

        // ==============================
        // Private Methods
        // ==============================

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