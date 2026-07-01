using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityBLL.Services.Abstraction.Payment;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentInfoController : ControllerBase
    {
        private readonly IPaymentInfoService _paymentInfoService;
        private readonly ILogger<PaymentInfoController> _logger;

        public PaymentInfoController(
            IPaymentInfoService paymentInfoService,
            ILogger<PaymentInfoController> logger)
        {
            _paymentInfoService = paymentInfoService ?? throw new ArgumentNullException(nameof(paymentInfoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// get payment info ( paymob api keys ) related to specific organization by organization id  
        /// </summary>
        // ─── GET api/paymentinfo/by-organization/{organizationId} ───────────────────

        [HttpGet("by-organization/{organizationId}")]
        public async Task<IActionResult> GetByOrganizationId(int organizationId)
        {
            try
            {
                _logger.LogInformation("Loading payment info for organization ID: {OrganizationId}", organizationId);

                var result = await _paymentInfoService.GetPaymentInfoByOrganizationIdAsync(organizationId);

                if (result == null)
                {
                    _logger.LogWarning("No payment info found for organization ID: {OrganizationId}", organizationId);
                    return NotFound(new { message = $"No payment info found for organization ID '{organizationId}'." });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization with ID {OrganizationId} not found.", organizationId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment info for organization ID: {OrganizationId}", organizationId);
                return StatusCode(500, new { message = "An error occurred while loading payment info." });
            }
        }
        /// <summary>
        /// get payment info ( paymob api keys ) related to specific organization by payment info id  
        /// </summary>
        // ─── GET api/paymentinfo/{paymentInfoId} ────────────────────────────────────

        [HttpGet("{paymentInfoId}")]
        public async Task<IActionResult> GetById(int paymentInfoId)
        {
            try
            {
                _logger.LogInformation("Loading payment info with ID: {PaymentInfoId}", paymentInfoId);

                var result = await _paymentInfoService.GetPaymentInfoByIdAsync(paymentInfoId);

                if (result == null)
                {
                    _logger.LogWarning("Payment info with ID {PaymentInfoId} not found.", paymentInfoId);
                    return NotFound(new { message = $"Payment info with ID '{paymentInfoId}' not found." });
                }

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment info with ID {PaymentInfoId} not found.", paymentInfoId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment info with ID: {PaymentInfoId}", paymentInfoId);
                return StatusCode(500, new { message = "An error occurred while loading payment info." });
            }
        }
        /// <summary>
        /// store payment info ( paymob api keys ) in db, related to specific organization   
        /// </summary>
        // ─── POST api/paymentinfo/{organizationId} ───────────────────────────────────

        [HttpPost]
       
        public async Task<IActionResult> Create([FromBody] CreatePaymentInfoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
               
                var created = await _paymentInfoService.CreatePaymentInfoAsync( dto);
                if (created == null)
                    return BadRequest(new {message="The Organization Id isn`t Valid."});

                _logger.LogInformation("Payment info created successfully with ID: {PaymentInfoId}",
                    created.Id);

                return Ok(new { message = "Payment info created successfully.", data = created });
            }
            
           
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment info ");
                return StatusCode(500, new { message = "An error occurred while creating payment info." });
            }
        }
        /// <summary>
        /// edit payment info ( paymob api keys ) related to specific organization by payment info id  
        /// </summary>
        // ─── PUT api/paymentinfo/{paymentInfoId} ────────────────────────────────────

        [HttpPut("{paymentInfoId}")]
     
        public async Task<IActionResult> Update(int paymentInfoId, [FromBody] UpdatePaymentInfoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Updating payment info with ID: {PaymentInfoId}", paymentInfoId);

                var updated = await _paymentInfoService.UpdatePaymentInfoAsync(paymentInfoId, dto);
                if (updated == null)
                    return BadRequest(new { message = "The OrganizationId isn`t Valid." });

                _logger.LogInformation("Payment info updated successfully with ID: {PaymentInfoId}", paymentInfoId);

                return Ok(new { message = "Payment info updated successfully.", data = updated });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment info with ID {PaymentInfoId} not found.", paymentInfoId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict updating payment info with ID: {PaymentInfoId}", paymentInfoId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment info with ID: {PaymentInfoId}", paymentInfoId);
                return StatusCode(500, new { message = "An error occurred while updating payment info." });
            }
        }
        /// <summary>
        /// delete payment info ( paymob api keys ) related to specific organization by payment info id  
        /// </summary>
        // ─── DELETE api/paymentinfo/{paymentInfoId} ──────────────────────────────────

        [HttpDelete("{paymentInfoId}")]
     
        public async Task<IActionResult> Delete(int paymentInfoId)
        {
            try
            {
                _logger.LogInformation("Deleting payment info with ID: {PaymentInfoId}", paymentInfoId);

                var paymentInfo = await _paymentInfoService.GetPaymentInfoByIdAsync(paymentInfoId);
                if (paymentInfo == null)
                    return NotFound(new { message = $"Payment info with ID '{paymentInfoId}' not found." });

                await _paymentInfoService.DeletePaymentInfoAsync(paymentInfoId);

                _logger.LogInformation("Payment info deleted successfully with ID: {PaymentInfoId}", paymentInfoId);

                return Ok(new { message = "Payment info deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment info with ID {PaymentInfoId} not found.", paymentInfoId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict deleting payment info with ID: {PaymentInfoId}", paymentInfoId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting payment info with ID: {PaymentInfoId}", paymentInfoId);
                return StatusCode(500, new { message = "An error occurred while deleting payment info." });
            }
        }
        /// <summary>
        /// restore deleted payment info ( paymob api keys ) related to specific organization by payment info id  
        /// </summary>
        // ─── POST api/paymentinfo/restore/{paymentInfoId} ────────────────────────────

        [HttpPost("restore/{paymentInfoId}")]
       
        public async Task<IActionResult> Restore(int paymentInfoId)
        {
            try
            {
                _logger.LogInformation("Restoring payment info with ID: {PaymentInfoId}", paymentInfoId);

          
               if( ! await _paymentInfoService.RestorePaymentInfoAsync(paymentInfoId))
                    return BadRequest(new { message = "PaymentInfo Id isn`t valid." });

                _logger.LogInformation("Payment info restored successfully with ID: {PaymentInfoId}", paymentInfoId);

                return Ok(new { message = "Payment info restored successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Payment info with ID {PaymentInfoId} not found.", paymentInfoId);
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict restoring payment info with ID: {PaymentInfoId}", paymentInfoId);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring payment info with ID: {PaymentInfoId}", paymentInfoId);
                return StatusCode(500, new { message = "An error occurred while restoring payment info." });
            }
        }
        /// <summary>
        /// check if the organization have payment info ( paymob api keys )  
        /// </summary>
        // ─── GET api/paymentinfo/has/{organizationId} ────────────────────────────────

        [HttpGet("has/{organizationId}")]
        public async Task<IActionResult> HasPaymentInfo(int organizationId)
        {
            try
            {
                _logger.LogInformation("Checking payment info existence for organization ID: {OrganizationId}", organizationId);

                var result = await _paymentInfoService.HasPaymentInfoAsync(organizationId);

                return Ok(new { organizationId, hasPaymentInfo = result });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization with ID {OrganizationId} not found.", organizationId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment info existence for organization ID: {OrganizationId}", organizationId);
                return StatusCode(500, new { message = "An error occurred while checking payment info." });
            }
        }
        /// <summary>
        /// check if the organization have payment info ( paymob api keys )  
        /// </summary>
        // ─── GET api/paymentinfo/validate/{organizationId} ───────────────────────────

        [HttpGet("validate/{organizationId}")]
        public async Task<IActionResult> ValidatePaymentInfo(int organizationId)
        {
            try
            {
                _logger.LogInformation("Validating payment info for organization ID: {OrganizationId}", organizationId);

                var isValid = await _paymentInfoService.ValidatePaymentInfoAsync(organizationId);

                return Ok(new { organizationId, isValid });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Organization with ID {OrganizationId} not found.", organizationId);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment info for organization ID: {OrganizationId}", organizationId);
                return StatusCode(500, new { message = "An error occurred while validating payment info." });
            }
        }
    }
}
