using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using TheCharityBLL.DTOs.AttachmentDTOs;
using TheCharityBLL.DTOs.DonatedItemDTOs;
using TheCharityBLL.DTOs.ItemImageDTOs;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.ViewModels;
using TheCharityDAL.Enums;

namespace TheCharityPL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonatedItemController : ControllerBase
    {
        private readonly IDonatedItemService _donatedItemService;
        public DonatedItemController(IDonatedItemService donatedItemService)
        {
            _donatedItemService = donatedItemService;
        }
        //donated items

        /// <summary> 
        /// Get all donated items 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
        {
            var result = await _donatedItemService.GetAllDonatedItems(includeDeleted);
            return Ok(result);
        }

        /// <summary> 
        /// Get donated item by ID 
        /// </summary>
        [HttpGet("{itemId:int}")]
        public async Task<IActionResult> GetById(int itemId)
        {
            var result = await _donatedItemService.GetDonatedItemById(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary> 
        /// Create a new donated item 
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDonatedItemDto dto)
        {
            var result = await _donatedItemService.CreateDonatedItem(dto);
            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result);

        }
        /// <summary> 
        /// Update donated item
        /// </summary>
        [HttpPut("{itemId:int}")]
        public async Task<IActionResult> Update(int itemId, [FromBody] UpdateDonatedItemDto dto)
        {
            var result = await _donatedItemService.UpdateDonatedItem(itemId, dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Soft delete donated item
        /// </summary>
        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> Delete(int itemId)
        {
            var result = await _donatedItemService.DeleteDonatedItem(itemId);
            return result.Success ? NoContent() : NotFound(result);
        }

        /// <summary>
        /// Restore deleted donated item
        /// </summary>
        [HttpPatch("{itemId:int}/restore")]
        public async Task<IActionResult> Restore(int itemId)
        {
            var result = await _donatedItemService.RestoreDonatedItem(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated items by organization
        /// </summary>
        [HttpGet("filter/organization/{organizationId:int}")]
        public async Task<IActionResult> GetByOrganization(int organizationId)
        {
            var result = await _donatedItemService.GetDonatedItemsByOrganization(organizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated items by donor
        /// </summary>
        [HttpGet("filter/donor/{donorId}")]
        public async Task<IActionResult> GetByDonor(string donorId)
        {
            var result = await _donatedItemService.GetDonatedItemsByDonor(donorId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated items by category
        /// </summary>
        [HttpGet("filter/category")]
        public async Task<IActionResult> GetByCategory([FromQuery] ItemCategory category)
        {
            var result = await _donatedItemService.GetDonatedItemsByCategory(category);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get available donated items
        /// </summary>
        [HttpGet("filter/available")]
        public async Task<IActionResult> GetAvailable()
        {
            var result = await _donatedItemService.GetAvailableDonatedItems();
            return Ok(result);
        }

        /// <summary>
        /// Get unavailable donated items
        /// </summary>
        [HttpGet("filter/unavailable")]
        public async Task<IActionResult> GetUnAvailable()
        {
            var result = await _donatedItemService.GetUnavailableDonatedItems();
            return Ok(result);
        }

        /// <summary>
        /// Get deleted donated items
        /// </summary>
        [HttpGet("filter/deleted")]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _donatedItemService.GetDeletedDonatedItems();
            return Ok(result);
        }

        /// <summary>
        /// Search donated items
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string searchTerm)
        {
            var result = await _donatedItemService.SearchDonatedItems(searchTerm);
            return result.Success ? Ok(result) : NotFound(result);
        }


        //images

        /// <summary>
        /// Get all images for a donated item
        /// </summary>
        [HttpGet("image/{itemId:int}")]
        public async Task<IActionResult> GetItemImages(int itemId)
        {
            var result = await _donatedItemService.GetItemImages(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get item image by ID
        /// </summary>
        [HttpGet("{imageId:int}/image")]
        public async Task<IActionResult> GetImageById(int imageId)
        {
            var result = await _donatedItemService.GetItemImageById(imageId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Upload a new item image
        /// </summary>
        [HttpPost("image")]
        public async Task<IActionResult> CreateImage([FromForm] CreateItemImageDto dto)
        {
            var result = await _donatedItemService.CreateItemImage(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete item image
        /// </summary>
        [HttpDelete("image/{imageId:int}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var result = await _donatedItemService.DeleteItemImage(imageId);
            return result.Success ? NoContent() : NotFound(result);
        }

        /// <summary>
        /// Get image count for a donated item
        /// </summary>
        [HttpGet("image/{itemId:int}/count")]
        public async Task<IActionResult> GetImageCount(int itemId)
        {
            var result = await _donatedItemService.GetItemImageCountByDonatedItem(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get primary image for a donated item
        /// </summary>
        [HttpGet("image/{itemId:int}/primary")]
        public async Task<IActionResult> GetPrimaryImage(int itemId)
        {
            var result = await _donatedItemService.GetPrimaryItemImageForDonatedItem(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }


        //attachment

        /// <summary>
        /// Get all attachments for a donated item
        /// </summary>
        [HttpGet("attachment/{itemId:int}/all")]
        public async Task<IActionResult> GetAllAttachments(int itemId)
        {
            var result = await _donatedItemService.GetAllAttachments(itemId);
            return Ok(result);
        }

        /// <summary>
        /// Get item attachments
        /// </summary>
        [HttpGet("attachment/{itemId:int}")]
        public async Task<IActionResult> GetItemAttachments(int itemId)
        {
            var result = await _donatedItemService.GetItemAttachments(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get recipient attachments for a donated item
        /// </summary>
        [HttpGet("attachment/{itemId:int}/recipient")]
        public async Task<IActionResult> GetItemRecipientAttachments(int itemId)
        {
            var result = await _donatedItemService.GetRecipientAttachments(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get attachment by ID 
        /// </summary>
        [HttpGet("{attachmentId:int}/attachment")]
        public async Task<IActionResult> GetAttachmentById(int attachmentId)
        {
            var result = await _donatedItemService.GetAttachmentById(attachmentId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Upload a new attachment
        /// </summary>
        [HttpPost("attachment")]
        public async Task<IActionResult> CreateAttachment([FromForm] CreateAttachmentDto dto)
        {
            var result = await _donatedItemService.CreateAttachment(dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Delete attachment
        /// </summary>
        [HttpDelete("attachment/{id:int}")]
        public async Task<IActionResult> DeleteAttachment(int id)
        {
            var result = await _donatedItemService.DeleteAttachment(id);
            return result.Success ? NoContent() : NotFound(result);
        }


        //advanced

        /// <summary>
        /// Update item availability
        /// </summary>
        [HttpPatch("{itemId:int}/availability")]
        public async Task<IActionResult> UpdateAvailability(int itemId, [FromQuery] bool isAvailable)
        {
            var result = await _donatedItemService.UpdateItemAvailability(itemId, isAvailable);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Mark item as available
        /// </summary>
        [HttpPatch("{itemId:int}/available")]
        public async Task<IActionResult> MarkAsAvailable(int itemId)
        {
            var result = await _donatedItemService.MarkItemAsAvailable(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Mark item as unavailable
        /// </summary>
        [HttpPatch("{itemId:int}/unavailable")]
        public async Task<IActionResult> MarkAsUnAvailable(int itemId)
        {
            var result = await _donatedItemService.MarkItemAsUnavailable(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }


        /// <summary>
        /// Update item category
        /// </summary>
        [HttpPatch("{itemId:int}/category")]
        public async Task<IActionResult> UpdateCategory(int itemId, [FromQuery] ItemCategory category)
        {
            var result = await _donatedItemService.UpdateItemCategory(itemId, category);
            return result.Success ? Ok(result) : NotFound(result);
        }

        //statistics

        /// <summary>
        /// Get total donated items count
        /// </summary>
        [HttpGet("count")]
        public async Task<IActionResult> GetTotalItemsCount()
        {
            var result = await _donatedItemService.GetTotalDonatedItemsCount();
            return Ok(result);
        }

        /// <summary>
        /// Get available donated items count
        /// </summary>
        [HttpGet("availabl/count")]
        public async Task<IActionResult> GetAvailablItemsCount()
        {
            var result = await _donatedItemService.GetAvailableDonatedItemsCount();
            return Ok(result);
        }

        /// <summary>
        /// Get donated items count by organization
        /// </summary>
        [HttpGet("organization/{organizationId:int}/count")]
        public async Task<IActionResult> GetItemsCountByOrganization(int organizationId)
        {
            var result = await _donatedItemService.GetDonatedItemsCountByOrganization(organizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated items count by donor
        /// </summary>
        [HttpGet("donor/{donorId}/count")]
        public async Task<IActionResult> GetItemsCountByDonor(string donorId)
        {
            var result = await _donatedItemService.GetDonatedItemsCountByDonor(donorId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated items count by category
        /// </summary>
        [HttpGet("category/count")]
        public async Task<IActionResult> GetItemsCountByCategory(ItemCategory category)
        {
            var result = await _donatedItemService.GetDonatedItemsCountByCategory(category);
            return Ok(result);
        }

        /// <summary>
        /// Get donated items count for all categories
        /// </summary>
        [HttpGet("categories/count/all")]
        public async Task<IActionResult> GetItemsCountToAllCategories()
        {
            var result = await _donatedItemService.GetDonatedItemsCountToAllCategories();
            return Ok(result);
        }

        /// <summary>
        /// Get top donors
        /// </summary>
        [HttpGet("top-donors")]
        public async Task<IActionResult> GetTopDonors(int top)
        {
            var result = await _donatedItemService.GetTopDonors(top);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get top organizations by donations
        /// </summary>
        [HttpGet("top-organization/donation")]
        public async Task<IActionResult> GetTopOrganizations(int top)
        {
            var result = await _donatedItemService.GetTopOrganizationsByDonations(top);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        // advanced queries

        /// <summary>
        /// Get recently donated items
        /// </summary>
        [HttpGet("items/recent")]
        public async Task<IActionResult> GetRecentItems([FromQuery] int days = 30)
        {
            var result = await _donatedItemService.GetRecentDonatedItems(days);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get donated items without images
        /// </summary>  
        [HttpGet("items/without-images")]
        public async Task<IActionResult> GetItemsWithoutImages()
        {
            var result = await _donatedItemService.GetDonatedItemsWithoutImages();
            return Ok(result);
        }

        /// <summary>
        /// Get donated items with attachments
        /// </summary>
        [HttpGet("items/with-attachments")]
        public async Task<IActionResult> GetItemsWithAttachments()
        {
            var result = await _donatedItemService.GetDonatedItemsWithAttachments();
            return Ok(result);
        }

        /// <summary>
        /// Get donated items by date range
        /// </summary>
        [HttpGet("items/date-range")]
        public async Task<IActionResult> GetItemsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _donatedItemService.GetDonatedItemsByDateRange(startDate, endDate);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Bulk update item categories
        /// </summary>
        [HttpPatch("bulk/category")]
        public async Task<IActionResult> BulkUpdateItemCategories([FromQuery] ItemCategory oldCategory, [FromQuery] ItemCategory newCategory)
        {
            var result = await _donatedItemService.BulkUpdateItemCategories(oldCategory, newCategory);
            return Ok(result);
        }

        /// <summary>
        /// Bulk mark organization items as unavailable
        /// </summary>
        [HttpPatch("bulk/unavailable")]
        public async Task<IActionResult> BulkMarkItemsAsUnavailable([FromQuery] int organizationId)
        {
            var result = await _donatedItemService.BulkMarkItemsAsUnavailable(organizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Delete old donated items
        /// </summary>
        [HttpDelete("delete-old")]
        public async Task<IActionResult> DeleteOldItems([FromQuery] int daysOld = 365)
        {
            var result = await _donatedItemService.DeleteOldDonatedItems(daysOld);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get donated item with images
        /// </summary>
        [HttpGet("{itemId:int}/with-images")]
        public async Task<IActionResult> GetItemWithImages(int itemId)
        {
            var result = await _donatedItemService.GetDonatedItemWithImages(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated item with attachments
        /// </summary>
        [HttpGet("{itemId:int}/with-attachments")]
        public async Task<IActionResult> GetItemWithAttachments(int itemId)
        {
            var result = await _donatedItemService.GetDonatedItemWithAttachments(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donated item details with donor and organization
        /// </summary>
        [HttpGet("{itemId:int}/details")]
        public async Task<IActionResult> GetItemWithDonorAndOrganization(int itemId)
        {
            var result = await _donatedItemService.GetDonatedItemWithDonorAndOrganization(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get most recent donated items
        /// </summary>
        [HttpGet("recent/limit")]
        public async Task<IActionResult> GetMostRecentItems([FromQuery] int limit = 10)
        {
            var result = await _donatedItemService.GetMostRecentDonatedItems(limit);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get donated items trend
        /// </summary>
        [HttpGet("trend")]
        public async Task<IActionResult> GetItemsTrend([FromQuery] int days = 30)
        {
            var result = await _donatedItemService.GetDonatedItemsTrend(days);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get donor's donated items history
        /// </summary>
        [HttpGet("donor/{donorId}/history")]
        public async Task<IActionResult> GetDonorDonatedItemsHistory(string donorId)
        {
            var result = await _donatedItemService.GetDonorDonatedItemsHistory(donorId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get total donated items count for a donor
        /// </summary>
        [HttpGet("donor/count/{donorId}")]
        public async Task<IActionResult> GetDonorTotalItemsCount(string donorId)
        {
            var result = await _donatedItemService.GetDonorTotalDonatedItemsCount(donorId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donor's most common donated category
        /// </summary>
        [HttpGet("donor/{donorId}/favorite-category")]
        public async Task<IActionResult> GetDonorMostCommonCategory(string donorId)
        {
            var result = await _donatedItemService.GetDonorMostCommonCategory(donorId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get organization inventory
        /// </summary>
        [HttpGet("organization/{organizationId}/inventory")]
        public async Task<IActionResult> GetOrganizationInventory(int organizationId)
        {
            var result = await _donatedItemService.GetOrganizationInventory(organizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get available donated items count for an organization
        /// </summary>
        [HttpGet("organization/{organizationId}/available-count")]
        public async Task<IActionResult> GetOrganizationAvailableItemsCount(int organizationId)
        {
            var result = await _donatedItemService.GetOrganizationAvailableItemsCount(organizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get organization inventory grouped by category
        /// </summary>
        [HttpGet("organization/{organizationId}/inventory-by-category")]
        public async Task<IActionResult> GetOrganizationInventoryByCategory(int organizationId)
        {
            var result = await _donatedItemService.GetOrganizationInventoryByCategory(organizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get total storage used by a donated item
        /// </summary>
        [HttpGet("{itemId:int}/storage")]
        public async Task<IActionResult> GetTotalFileSize(int itemId)
        {
            var result = await _donatedItemService.GetTotalFileSize(itemId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get total storage used by all donated items
        /// </summary>
        [HttpGet("storage/total")]
        public async Task<IActionResult> GetTotalStorageUsed()
        {
            var result = await _donatedItemService.GetTotalStorageUsed();
            return Ok(result);
        }

        /// <summary>
        /// Get large attachments
        /// </summary>
        [HttpGet("attachments/large")]
        public async Task<IActionResult> GetLargeAttachments([FromQuery] long sizeThreshold)
        {
            var result = await _donatedItemService.GetLargeAttachments(sizeThreshold);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get donated items by multiple categories
        /// </summary>
        [HttpPost("categories/multiple")]
        public async Task<IActionResult> GetItemsByMultipleCategories([FromBody] IEnumerable<ItemCategory> categories)
        {
            var result = await _donatedItemService.GetItemsByMultipleCategories(categories);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get category distribution percentage
        /// </summary>
        [HttpGet("categories/distribution")]
        public async Task<IActionResult> GetCategoryDistributionPercentage()
        {
            var result = await _donatedItemService.GetCategoryDistributionPercentage();
            return Ok(result);
        }

        /// <summary>
        /// Get most popular categories
        /// </summary>
        [HttpGet("categories/popular")]
        public async Task<IActionResult> GetMostPopularCategories([FromQuery] int limit = 2)
        {
            var result = await _donatedItemService.GetMostPopularCategories(limit);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Search available items by category
        /// </summary>
        [HttpGet("search/category")]
        public async Task<IActionResult> SearchAvailableItemsByCategory([FromQuery] string searchTerm, [FromQuery] ItemCategory category)
        {
            var result = await _donatedItemService.SearchAvailableItemsByCategory(searchTerm, category);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get organization items by category
        /// </summary>
        [HttpGet("organization/{organizationId}/category")]
        public async Task<IActionResult> GetItemsByOrganizationAndCategory(int organizationId, [FromQuery] ItemCategory category)
        {
            var result = await _donatedItemService.GetItemsByOrganizationAndCategory(organizationId, category);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get donor items within a date range
        /// </summary>
        [HttpGet("donor/{donorId}/date-range")]
        public async Task<IActionResult> GetItemsByDonorAndDateRange(string donorId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _donatedItemService.GetItemsByDonorAndDateRange(donorId, startDate, endDate);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Transfer donated item to another organization
        /// </summary>
        [HttpPatch("{itemId:int}/transfer")]
        public async Task<IActionResult> TransferItemToOrganization(int itemId, [FromQuery] int newOrganizationId)
        {
            var result = await _donatedItemService.TransferItemToOrganization(itemId, newOrganizationId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Update donated item donor
        /// </summary>
        [HttpPatch("{itemId:int}/donor")]
        public async Task<IActionResult> UpdateItemDonor(int itemId, [FromQuery] string newDonorId)
        {
            var result = await _donatedItemService.UpdateItemDonor(itemId, newDonorId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Get recently updated donated items
        /// </summary>
        [HttpGet("recently-updated")]
        public async Task<IActionResult> GetRecentlyUpdatedItems([FromQuery] int hours = 24)
        {
            var result = await _donatedItemService.GetRecentlyUpdatedItems(hours);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get donor activity
        /// </summary>
        [HttpGet("activity/donors")]
        public async Task<IActionResult> GetActivityByDonor([FromQuery] int days = 30)
        {
            var result = await _donatedItemService.GetActivityByDonor(days);
            return result.Success ? Ok(result) : BadRequest(result);
        }

    }
}
