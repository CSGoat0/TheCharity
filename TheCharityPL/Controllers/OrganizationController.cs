using Microsoft.AspNetCore.Mvc;

using TheCharityBLL.Authorization.Attributes;

using TheCharityBLL.DTOs.OrganizationContactMethodDTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Enums;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]//we must specifc roles and policies for each endpoint
    [ApiController]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        //organization

        /// <summary>
        /// Get all organizations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]PaginationParametersDto filterDto,[FromQuery] bool includeDeleted = false)
        {
            var result = await _organizationService.GetAllOrganizationsAsync(filterDto,includeDeleted);
            return Ok(result);
        }

        /// <summary>
        /// Get organization by ID
        /// </summary>
        [HttpGet("{orgId:int}")]
        public async Task<IActionResult> GetById(int orgId)
        {
            var result = await _organizationService.GetOrganizationByIdAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get organization details
        /// </summary>
        [HttpGet("{orgId:int}/details")]
        public async Task<IActionResult> GetDetails(int orgId)
        {
            var result = await _organizationService.GetOrganizationDetailsAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create a new organization
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationDto dto)
        {
            var result = await _organizationService.CreateOrganizationAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { orgId = result.Data.Id }, result);
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut("{orgId:int}")]
        public async Task<IActionResult> Update(int orgId, [FromBody] UpdateOrganizationDto dto)
        {
            var result = await _organizationService.UpdateOrganizationAsync(orgId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Soft delete organization
        /// </summary>
        [HttpDelete("{orgId:int}")]
        public async Task<IActionResult> Delete(int orgId)
        {
            var result = await _organizationService.DeleteOrganizationAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Restore deleted organization
        /// </summary>
        [HttpPatch("{orgId:int}/restore")]
        public async Task<IActionResult> Restore(int orgId)
        {
            var result = await _organizationService.RestoreOrganizationAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get all deleted organizations
        /// </summary>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetDeletedOrganizationsAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Get organizations for dropdown list
        /// </summary>
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsDropDownAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Organization name exists
        /// </summary>
        [HttpGet("name-exists")]
        public async Task<IActionResult> OrganizationNameExists([FromQuery]string name)
        {
            var result = await _organizationService.OrganizationNameExistsAsync(name);
            return Ok(result);
        }

        /// <summary>
        /// Search organizations by keyword
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] PaginationParametersDto filterDto, [FromQuery] string term)
        {
            var result = await _organizationService.SearchOrganizationsAsync(filterDto,term);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get organization by name
        /// </summary>
        [HttpGet("filter/by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            var result = await _organizationService.GetOrganizationByNameAsync(name);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get organizations by address
        /// </summary>
        [HttpGet("filter/by-address")]
        public async Task<IActionResult> GetByAddress([FromQuery] PaginationParametersDto filterDto,[FromQuery] string address)
        {
            var result = await _organizationService.GetOrganizationsByAddressAsync(filterDto,address);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get recently registered organizations
        /// </summary>
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] PaginationParametersDto filterDto,[FromQuery] int days)
        {
            var result = await _organizationService.GetRecentlyRegisteredOrganizationsAsync(filterDto,days);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get total organizations count
        /// </summary>
        [HttpGet("count/total")]
        public async Task<IActionResult> GetTotalCount()
        {
            var result = await _organizationService.GetTotalOrganizationsCountAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get active organizations count
        /// </summary>
        [HttpGet("count/active")]
        public async Task<IActionResult> GetActiveCount()
        {
            var result = await _organizationService.GetActiveOrganizationsCountAsync();
            return Ok(result);
        }

        //contact methods

        /// <summary>
        /// Get contact methods for an organization
        /// </summary>
        [HttpGet("{orgId}/contact-methods")]
        public async Task<IActionResult> GetOrganizationContactMethods([FromQuery] PaginationParametersDto filterDto,int orgId)
        {
            var result = await _organizationService.GetOrganizationContactMethodsAsync(filterDto,orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get contact method by ID
        /// </summary>
        [HttpGet("contact-methods/{contactId}")]
        public async Task<IActionResult> GetContactMethodById(int contactId)
        {
            var result = await _organizationService.GetContactMethodByIdAsync(contactId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Create organization contact method
        /// </summary>
        [HttpPost("contact-methods")]
        public async Task<IActionResult> CreateContactMethod(CreateOrgContactMethodDto dto)
        {
            var result = await _organizationService.CreateContactMethodAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Update organization contact method
        /// </summary>
        [HttpPut("contact-methods/{contactId:int}")]
        public async Task<IActionResult> UpdateContactMethod(int contactId, UpdateOrgContactMethodDto dto)
        {
            var result = await _organizationService.UpdateContactMethodAsync(contactId, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete organization contact method
        /// </summary>
        [HttpDelete("contact-methods/{contactId}")]
        public async Task<IActionResult> DeleteContactMethod(int contactId)
        {
            var result = await _organizationService.DeleteContactMethodAsync(contactId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Restore deleted contact method
        /// </summary>
        [HttpPost("contact-methods/restore/{contactId}")]
        public async Task<IActionResult> RestoreContactMethod(int contactId)
        {
            var result = await _organizationService.RestoreContactMethodAsync(contactId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get contact methods by type
        /// </summary>
        [HttpGet("{orgId:int}/contact-type")]
        public async Task<IActionResult> GetContactMethodsByType([FromQuery] PaginationParametersDto filterDto,int orgId, ContactType type)
        {
            var result = await _organizationService.GetContactMethodsByTypeAsync(filterDto,orgId, type);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get contact methods count by type
        /// </summary>
        [HttpGet("{orgId:int}/contact-type/count")]
        public async Task<IActionResult> GetContactMethodCountByType(int orgId, ContactType type)
        {
            var result = await _organizationService.GetContactMethodCountByTypeAsync(orgId, type);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get organizations by contact type
        /// </summary>
        [HttpGet("contact-type/{type}")]
        public async Task<IActionResult> GetOrganizationsByContactType([FromQuery] PaginationParametersDto filterDto,ContactType type)
        {
            var result = await _organizationService.GetOrganizationsByContactTypeAsync(filterDto,type);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get organizations without payment information
        /// </summary>
        [HttpGet("payment/none")]
        public async Task<IActionResult> GetOrganizationsWithoutPaymentInfo([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithoutPaymentInfoAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Get organizations with valid payment information
        /// </summary>
        [HttpGet("payment/valid")]
        public async Task<IActionResult> GetOrganizationsWithValidPaymentInfo([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithValidPaymentInfoAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Get organizations ordered by last payment update
        /// </summary>
        [HttpGet("payment/last-update")]
        public async Task<IActionResult> GetLastPaymentUpdates()
        {
            var result = await _organizationService.GetOrganizationLastPaymentUpdateAsync();
            return Ok(result);
        }

        //camaign

        /// <summary>
        /// Get organizations with at least the specified number of campaigns
        /// </summary>
        [HttpGet("campaigns/min-count")]
        public async Task<IActionResult> GetOrganizationsByCampaignCount([FromQuery] PaginationParametersDto filterDto, int minCampaigns)
        {
            var result = await _organizationService.GetOrganizationsByCampaignCountAsync(filterDto,minCampaigns);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        
        /// <summary>
        /// Get organizations with active campaigns
        /// </summary>  
        [HttpGet("campaigns/active")]
        public async Task<IActionResult> GetOrganizationsWithActiveCampaigns([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithActiveCampaignsAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Get organizations with completed campaigns
        /// </summary>
        [HttpGet("campaigns/completed")]
        public async Task<IActionResult> GetOrganizationsWithCompletedCampaigns([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithCompletedCampaignsAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Get organizations without campaigns
        /// </summary>
        [HttpGet("campaigns/none")]
        public async Task<IActionResult> GetOrganizationsWithoutCampaigns([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithoutCampaignsAsync(filterDto);
            return Ok(result);
        }

        // ==============================
        // Organization Admin Management
        // ==============================

        /// <summary>
        /// Assign an organization admin (SuperAdmin only)
        /// </summary>
        [HttpPost("{orgId}/admin")]
        [IsSuperAdmin] // Only SuperAdmin can assign organization admin
        public async Task<IActionResult> AssignOrganizationAdmin(int orgId, [FromBody] AssignAdminRequest request)
        {
            var result = await _organizationService.AssignOrganizationAdminAsync(orgId, request.UserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Remove organization admin (SuperAdmin only)
        /// </summary>
        [HttpDelete("{orgId}/admin")]
        [IsSuperAdmin]
        public async Task<IActionResult> RemoveOrganizationAdmin(int orgId)
        {
            var result = await _organizationService.RemoveOrganizationAdminAsync(orgId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Transfer organization admin to another user (SuperAdmin only)
        /// </summary>
        [HttpPost("{orgId}/admin/transfer")]
        [IsSuperAdmin]
        public async Task<IActionResult> TransferOrganizationAdmin(int orgId, [FromBody] AssignAdminRequest request)
        {
            var result = await _organizationService.TransferOrganizationAdminAsync(orgId, request.UserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get organization admin
        /// </summary>
        [HttpGet("{orgId}/admin")]
        [CanManageOrganization] // OrganizationAdmin + SuperAdmin can view
        public async Task<IActionResult> GetOrganizationAdmin(int orgId)
        {
            var result = await _organizationService.GetOrganizationAdminAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // ==============================
        // Sub-Admin Management
        // ==============================

        /// <summary>
        /// Add a sub-admin to an organization (SuperAdmin + OrganizationAdmin only)
        /// </summary>
        [HttpPost("{orgId}/sub-admins")]
        [CanManageSubAdmins] // SuperAdmin + OrganizationAdmin (NOT SubAdmin)
        public async Task<IActionResult> AddSubAdmin(int orgId, [FromBody] AssignAdminRequest request)
        {
            var result = await _organizationService.AddSubAdminAsync(orgId, request.UserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Remove a sub-admin from an organization (SuperAdmin + OrganizationAdmin only)
        /// </summary>
        [HttpDelete("{orgId}/sub-admins/{userId}")]
        [CanManageSubAdmins] // SuperAdmin + OrganizationAdmin (NOT SubAdmin)
        public async Task<IActionResult> RemoveSubAdmin(int orgId, string userId)
        {
            var result = await _organizationService.RemoveSubAdminAsync(orgId, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all sub-admins of an organization
        /// </summary>
        [HttpGet("{orgId}/sub-admins")]
        [CanManageOrganization] // SuperAdmin + OrganizationAdmin + SubAdmin can view
        public async Task<IActionResult> GetSubAdmins(int orgId)
        {
            var result = await _organizationService.GetOrganizationSubAdminsAsync(orgId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Check if a user is a sub-admin of an organization
        /// </summary>
        [HttpGet("{orgId}/sub-admins/{userId}/check")]
        [CanManageOrganization] // SuperAdmin + OrganizationAdmin + SubAdmin
        public async Task<IActionResult> IsUserSubAdmin(int orgId, string userId)
        {
            var result = await _organizationService.IsUserSubAdminAsync(orgId, userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
