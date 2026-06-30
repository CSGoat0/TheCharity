using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery]PaginationParametersDto filterDto,[FromQuery] bool includeDeleted = false)
        {
            var result = await _organizationService.GetAllOrganizationsAsync(filterDto,includeDeleted);
            return Ok(result);
        }

        [HttpGet("{orgId:int}")]
        public async Task<IActionResult> GetById(int orgId)
        {
            var result = await _organizationService.GetOrganizationByIdAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{orgId:int}/details")]
        public async Task<IActionResult> GetDetails(int orgId)
        {
            var result = await _organizationService.GetOrganizationDetailsAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrganizationDto dto)
        {
            var result = await _organizationService.CreateOrganizationAsync(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { orgId = result.Data.Id }, result);
        }

        [HttpPut("{orgId:int}")]
        public async Task<IActionResult> Update(int orgId, [FromBody] UpdateOrganizationDto dto)
        {
            var result = await _organizationService.UpdateOrganizationAsync(orgId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{orgId:int}")]
        public async Task<IActionResult> Delete(int orgId)
        {
            var result = await _organizationService.DeleteOrganizationAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPatch("{orgId:int}/restore")]
        public async Task<IActionResult> Restore(int orgId)
        {
            var result = await _organizationService.RestoreOrganizationAsync(orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeleted([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetDeletedOrganizationsAsync(filterDto);
            return Ok(result);
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var result = await _organizationService.GetOrganizationsDropDownAsync();
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

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] PaginationParametersDto filterDto, [FromQuery] string term)
        {
            var result = await _organizationService.SearchOrganizationsAsync(filterDto,term);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("filter/by-name")]
        public async Task<IActionResult> GetByName([FromQuery] string name)
        {
            var result = await _organizationService.GetOrganizationByNameAsync(name);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("filter/by-address")]
        public async Task<IActionResult> GetByAddress([FromQuery] PaginationParametersDto filterDto,[FromQuery] string address)
        {
            var result = await _organizationService.GetOrganizationsByAddressAsync(filterDto,address);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] PaginationParametersDto filterDto,[FromQuery] int days)
        {
            var result = await _organizationService.GetRecentlyRegisteredOrganizationsAsync(filterDto,days);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("count/total")]
        public async Task<IActionResult> GetTotalCount()
        {
            var result = await _organizationService.GetTotalOrganizationsCountAsync();
            return Ok(result);
        }

        [HttpGet("count/active")]
        public async Task<IActionResult> GetActiveCount()
        {
            var result = await _organizationService.GetActiveOrganizationsCountAsync();
            return Ok(result);
        }

        //contact methods

        [HttpGet("{orgId}/contact-methods")]
        public async Task<IActionResult> GetOrganizationContactMethods([FromQuery] PaginationParametersDto filterDto,int orgId)
        {
            var result = await _organizationService.GetOrganizationContactMethodsAsync(filterDto,orgId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("contact-methods/{contactId}")]
        public async Task<IActionResult> GetContactMethodById(int contactId)
        {
            var result = await _organizationService.GetContactMethodByIdAsync(contactId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("contact-methods")]
        public async Task<IActionResult> CreateContactMethod(CreateOrgContactMethodDto dto)
        {
            var result = await _organizationService.CreateContactMethodAsync(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPut("contact-methods/{contactId:int}")]
        public async Task<IActionResult> UpdateContactMethod(int contactId, UpdateOrgContactMethodDto dto)
        {
            var result = await _organizationService.UpdateContactMethodAsync(contactId, dto);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("contact-methods/{contactId}")]
        public async Task<IActionResult> DeleteContactMethod(int contactId)
        {
            var result = await _organizationService.DeleteContactMethodAsync(contactId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpPost("contact-methods/restore/{contactId}")]
        public async Task<IActionResult> RestoreContactMethod(int contactId)
        {
            var result = await _organizationService.RestoreContactMethodAsync(contactId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{orgId:int}/contact-type")]
        public async Task<IActionResult> GetContactMethodsByType([FromQuery] PaginationParametersDto filterDto,int orgId, ContactType type)
        {
            var result = await _organizationService.GetContactMethodsByTypeAsync(filterDto,orgId, type);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("{orgId:int}/contact-type/count")]
        public async Task<IActionResult> GetContactMethodCountByType(int orgId, ContactType type)
        {
            var result = await _organizationService.GetContactMethodCountByTypeAsync(orgId, type);
            return result.Success ? Ok(result) : NotFound(result);
        }

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

        [HttpGet("payment/valid")]
        public async Task<IActionResult> GetOrganizationsWithValidPaymentInfo([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithValidPaymentInfoAsync(filterDto);
            return Ok(result);
        }

        [HttpGet("payment/last-update")]
        public async Task<IActionResult> GetLastPaymentUpdates()
        {
            var result = await _organizationService.GetOrganizationLastPaymentUpdateAsync();
            return Ok(result);
        }

        //camaign

        [HttpGet("campaigns/min-count")]
        public async Task<IActionResult> GetOrganizationsByCampaignCount([FromQuery] PaginationParametersDto filterDto, int minCampaigns)
        {
            var result = await _organizationService.GetOrganizationsByCampaignCountAsync(filterDto,minCampaigns);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("campaigns/active")]
        public async Task<IActionResult> GetOrganizationsWithActiveCampaigns([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithActiveCampaignsAsync(filterDto);
            return Ok(result);
        }

        [HttpGet("campaigns/completed")]
        public async Task<IActionResult> GetOrganizationsWithCompletedCampaigns([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithCompletedCampaignsAsync(filterDto);
            return Ok(result);
        }

        [HttpGet("campaigns/none")]
        public async Task<IActionResult> GetOrganizationsWithoutCampaigns([FromQuery] PaginationParametersDto filterDto)
        {
            var result = await _organizationService.GetOrganizationsWithoutCampaignsAsync(filterDto);
            return Ok(result);
        }
    }
}
