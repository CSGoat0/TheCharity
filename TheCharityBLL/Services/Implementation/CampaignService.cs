using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Extensions;
﻿using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.CampaignEvents;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.ViewModels;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Repository
{
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IDonationRepository _donationRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly CampaignMapper _mapper;

        public CampaignService(
            ICampaignRepository campaignRepository,
            IDonationRepository donationRepository,
            IOrganizationRepository organizationRepository,
            IEventDispatcher eventDispatcher
            )
        {
            _campaignRepository = campaignRepository;
            _donationRepository = donationRepository;
            _organizationRepository = organizationRepository;
            _eventDispatcher = eventDispatcher;
            _mapper = new CampaignMapper();
        }

        // ===== Base CRUD Operations =====

        public async Task<ServiceResponse<CampaignResponseDto>> GetCampaignByIdAsync(int id)
        {
            var campaign = await _campaignRepository.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return new ServiceResponse<CampaignResponseDto>
                {
                    Success = false,
                    Message = $"Campaign with ID {id} not found."
                };
            }

            var response = _mapper.MapToResponseDto(campaign);
            return new ServiceResponse<CampaignResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Campaign retrieved successfully."
            };
        }

        public async Task<ServiceResponse<CampaignDetailsResponseDto>> GetCampaignDetailsByIdAsync(int id)
        {
            var campaign = await _campaignRepository.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return new ServiceResponse<CampaignDetailsResponseDto>
                {
                    Success = false,
                    Message = $"Campaign with ID {id} not found."
                };
            }

            var donations = await _donationRepository.GetDonationsByCampaignAsync(id);
            var recentDonations = donations
                .OrderByDescending(d => d.RegistrationDate)
                .Take(10)
                .Select(d => new DonationBasicDto
                {
                    Id = d.Id,
                    Amount = d.Amount,
                    RegistrationDate = d.RegistrationDate,
                    UserName = d.User?.UserName ?? "Anonymous"
                }).ToList();

            var response = new CampaignDetailsResponseDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                ImgPath = campaign.ImgPath,
                Target = campaign.Target,
                Achieved = campaign.Achieved,
                Status = campaign.Status,
                Type = campaign.Type,
                IsDeleted = campaign.IsDeleted,
                RegistrationDate = campaign.RegistrationDate,
                UpdatedOn = campaign.UpdatedOn,
                AchievementPercentage = CalculatePercentage(campaign.Achieved, campaign.Target),
                RemainingAmount = (campaign.Target ?? 0) - (campaign.Achieved ?? 0),
                TotalDonationsCount = donations.Count(),
                RecentDonations = recentDonations
            };

            return new ServiceResponse<CampaignDetailsResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Campaign details retrieved successfully."
            };
        }

        public async Task<ServiceResponse<bool>> UpdateCampaignAsync(UpdateCampaignDto updateDto)
        {
            var existingCampaign = await _campaignRepository.GetCampaignByIdAsync(updateDto.Id);
            if (existingCampaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Campaign with ID {updateDto.Id} not found."
                };
            }

            // Prevent target update for active campaigns
            if (existingCampaign.Status == CampaignStatus.Active &&
                updateDto.Target.HasValue &&
                existingCampaign.Target != updateDto.Target)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Cannot update target amount for an active campaign."
                };
            }

            // Apply updates using entity methods
            existingCampaign.EditTitle(updateDto.Title);
            existingCampaign.EditDescription(updateDto.Description);
            existingCampaign.EditImage(updateDto.ImgPath);
            existingCampaign.EditTarget(updateDto.Target);
            existingCampaign.EditType(updateDto.Type);

            await _campaignRepository.UpdateCampaignAsync(existingCampaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Campaign updated successfully."
            };
        }

        public async Task<ServiceResponse<bool>> DeleteCampaignAsync(int id)
        {
            var existingCampaign = await _campaignRepository.GetCampaignByIdAsync(id);
            if (existingCampaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Campaign with ID {id} not found."
                };
            }

            existingCampaign.Delete();
            await _campaignRepository.UpdateCampaignAsync(existingCampaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Campaign deleted successfully."
            };
        }

        public async Task<ServiceResponse<bool>> RestoreCampaignAsync(int id)
        {
            await _campaignRepository.RestoreCampaignAsync(id);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Campaign restored successfully."
            };
        }

        // ===== Solo Campaign Operations =====

        public async Task<ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>> GetAllSoloCampaignsAsync(PaginationParametersDto parametersDto,bool includeDeleted = false)
        {
            (IEnumerable<SoloCampaign> Data, int TotalCount) campaigns;

            if (includeDeleted)
            {
                var allCampaigns = await _campaignRepository.GetAllCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, true);
                campaigns.Data = allCampaigns.Data.OfType<SoloCampaign>();
                campaigns.TotalCount = allCampaigns.TotalCount;
            }
            else
            {
                campaigns = await _campaignRepository.GetAllSoloCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize);
            }

            var campaignsDtos=_mapper.MapToSoloResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Solo campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<SoloCampaignResponseDto>> GetSoloCampaignByIdAsync(int id)
        {
            var campaign = await _campaignRepository.GetSoloCampaignByIdAsync(id);
            if (campaign == null)
            {
                return new ServiceResponse<SoloCampaignResponseDto>
                {
                    Success = false,
                    Message = $"Solo campaign with ID {id} not found."
                };
            }

            var response = _mapper.MapToSoloResponseDto(campaign);

            return new ServiceResponse<SoloCampaignResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Solo campaign retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> CreateSoloCampaignAsync(CreateSoloCampaignDto createDto)
        {
            // Validate organization exists
            if (!await _organizationRepository.OrganizationExistsAsync(createDto.OrganizationId))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"Organization with ID {createDto.OrganizationId} not found."
                };
            }

            // Validate title uniqueness? (Optional - add if needed)

            var campaign = _mapper.MapToSoloEntity(createDto);
            var created = await _campaignRepository.AddSoloCampaignAsync(campaign);

            await _eventDispatcher.DispatchAsync(new CampaignCreatedEvent
            {
                Campaign = created
            });

            return new ServiceResponse<int>
            {
                Success = true,
                Data = created.Id,
                Message = "Solo campaign created successfully."
            };
        }

        public async Task<ServiceResponse<bool>> UpdateSoloCampaignAsync(UpdateSoloCampaignDto updateDto)
        {
            var existingCampaign = await _campaignRepository.GetSoloCampaignByIdAsync(updateDto.Id);
            if (existingCampaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Solo campaign with ID {updateDto.Id} not found."
                };
            }

            // Prevent target update for active campaigns
            if (existingCampaign.Status == CampaignStatus.Active &&
                updateDto.Target.HasValue &&
                existingCampaign.Target != updateDto.Target)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Cannot update target amount for an active campaign."
                };
            }

            // Apply updates using entity methods
            existingCampaign.EditTitle(updateDto.Title);
            existingCampaign.EditDescription(updateDto.Description);
            existingCampaign.EditImage(updateDto.ImgPath);
            existingCampaign.EditTarget(updateDto.Target);
            existingCampaign.EditType(updateDto.Type);

            await _campaignRepository.UpdateSoloCampaignAsync(existingCampaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Solo campaign updated successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>> GetSoloCampaignsByOrganizationIdAsync(PaginationParametersDto parametersDto, int organizationId)
        {
            if (!await _organizationRepository.OrganizationExistsAsync(organizationId))
            {
                return new ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found."
                };
            }

            var campaigns = await _campaignRepository.GetSoloCampaignsByOrganizationIdAsync(parametersDto.PageNumber,parametersDto.PageSize,organizationId);
            var campaignsDto = _mapper.MapToSoloResponseDtos(campaigns.Data);
            
            var response = campaigns.ToPagedResult(campaignsDto, parametersDto);

            return new ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Solo campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>> GetSoloCampaignsByStatusAsync(PaginationParametersDto parametersDto, CampaignStatus status)
        {
            var campaigns = await _campaignRepository.GetSoloCampaignsByStatusAsync(parametersDto.PageNumber, parametersDto.PageSize, status);
            var campaignsDto = _mapper.MapToSoloResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDto, parametersDto);

            return new ServiceResponse<PagedResultDto<SoloCampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Solo campaigns with status {status} retrieved successfully."
            };
        }

        // ===== Shared Campaign Operations =====

        public async Task<ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>> GetAllSharedCampaignsAsync(PaginationParametersDto parametersDto,bool includeDeleted = false)
        {
            (IEnumerable<SharedCampaign> Data,int TotalCount) campaigns;

            if (includeDeleted)
            {
                var allCampaigns = await _campaignRepository.GetAllCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, true);
                campaigns.Data = allCampaigns.Data.OfType<SharedCampaign>();
                campaigns.TotalCount = allCampaigns.TotalCount;
            }
            else
            {
                campaigns = await _campaignRepository.GetAllSharedCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize);
            }

            var campaignsDtos = _mapper.MapToSharedResponseDtos(campaigns.Data);
            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Shared campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<SharedCampaignResponseDto>> GetSharedCampaignByIdAsync(int id)
        {
            var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(id);
            if (campaign == null)
            {
                return new ServiceResponse<SharedCampaignResponseDto>
                {
                    Success = false,
                    Message = $"Shared campaign with ID {id} not found."
                };
            }

            var response = _mapper.MapToSharedResponseDto(campaign);

            return new ServiceResponse<SharedCampaignResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Shared campaign retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> CreateSharedCampaignAsync(CreateSharedCampaignDto createDto)
        {
            var campaign = _mapper.MapToSharedEntity(createDto);

            // Add organizations if provided
            if (createDto.OrganizationIds != null && createDto.OrganizationIds.Any())
            {
                foreach (var orgId in createDto.OrganizationIds)
                {
                    var organization = await _organizationRepository.GetOrganizationByIdAsync(orgId);
                    if (organization != null)
                    {
                        campaign.AddOrganization(organization);
                    }
                }
            }

            var created = await _campaignRepository.AddSharedCampaignAsync(campaign);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = created.Id,
                Message = "Shared campaign created successfully."
            };
        }

        public async Task<ServiceResponse<bool>> UpdateSharedCampaignAsync(UpdateSharedCampaignDto updateDto)
        {
            var existingCampaign = await _campaignRepository.GetSharedCampaignByIdAsync(updateDto.Id);
            if (existingCampaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Shared campaign with ID {updateDto.Id} not found."
                };
            }

            // Prevent target update for active campaigns
            if (existingCampaign.Status == CampaignStatus.Active &&
                updateDto.Target.HasValue &&
                existingCampaign.Target != updateDto.Target)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Cannot update target amount for an active campaign."
                };
            }

            // Apply updates using entity methods
            existingCampaign.EditTitle(updateDto.Title);
            existingCampaign.EditDescription(updateDto.Description);
            existingCampaign.EditImage(updateDto.ImgPath);
            existingCampaign.EditTarget(updateDto.Target);
            existingCampaign.EditType(updateDto.Type);

            await _campaignRepository.UpdateSharedCampaignAsync(existingCampaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Shared campaign updated successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>> GetSharedCampaignsByOrganizationIdAsync(PaginationParametersDto parametersDto, int organizationId)
        {
            if (!await _organizationRepository.OrganizationExistsAsync(organizationId))
            {
                return new ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found."
                };
            }

            var campaigns = await _campaignRepository.GetSharedCampaignsByOrganizationIdAsync(parametersDto.PageNumber, parametersDto.PageSize, organizationId);
            var campaignsDtos = _mapper.MapToSharedResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Shared campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>> GetSharedCampaignsByStatusAsync(PaginationParametersDto parametersDto, CampaignStatus status)
        {
            var campaigns = await _campaignRepository.GetSharedCampaignsByStatusAsync(parametersDto.PageNumber, parametersDto.PageSize, status);
            var campaignsDtos = _mapper.MapToSharedResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<SharedCampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Shared campaigns with status {status} retrieved successfully."
            };
        }

        // ===== Shared Campaign Organization Management =====

        public async Task<ServiceResponse<bool>> AddOrganizationToSharedCampaignAsync(int sharedCampaignId, int organizationId)
        {
            var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(sharedCampaignId);
            if (campaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Shared campaign with ID {sharedCampaignId} not found."
                };
            }

            var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
            if (organization == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found."
                };
            }

            campaign.AddOrganization(organization);
            await _campaignRepository.UpdateSharedCampaignAsync(campaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Organization added to shared campaign successfully."
            };
        }

        public async Task<ServiceResponse<bool>> RemoveOrganizationFromSharedCampaignAsync(int sharedCampaignId, int organizationId)
        {
            var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(sharedCampaignId);
            if (campaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Shared campaign with ID {sharedCampaignId} not found."
                };
            }

            var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
            if (organization == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found."
                };
            }

            campaign.RemoveOrganization(organization);
            await _campaignRepository.UpdateSharedCampaignAsync(campaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Organization removed from shared campaign successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetOrganizationCountForSharedCampaignAsync(int sharedCampaignId)
        {
            var count = await _campaignRepository.GetOrganizationCountForSharedCampaignAsync(sharedCampaignId);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Organization count retrieved successfully."
            };
        }

        // ===== Campaign Progress Operations =====

        public async Task<ServiceResponse<bool>> UpdateCampaignMoneyAsync(int campaignId, double achievedAmount)
        {
            if (achievedAmount < 0)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Achieved amount cannot be negative."
                };
            }

            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Campaign with ID {campaignId} not found."
                };
            }

            campaign.UpdateMoneyAchieved(achievedAmount);
            await _campaignRepository.UpdateCampaignAsync(campaign);

            // Check if target is reached
            if (campaign.Target.HasValue && campaign.Achieved >= campaign.Target)
            {
                campaign.UpdateStatus(CampaignStatus.Completed);
                await _campaignRepository.UpdateCampaignAsync(campaign);
            }

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Campaign money updated successfully."
            };
        }

        public async Task<ServiceResponse<bool>> IncrementCampaignMoneyAsync(int campaignId, double amount)
        {
            if (amount <= 0)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Amount must be positive."
                };
            }

            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Campaign with ID {campaignId} not found."
                };
            }

            var newAmount = (campaign.Achieved ?? 0) + amount;
            campaign.UpdateMoneyAchieved(newAmount);
            await _campaignRepository.UpdateCampaignAsync(campaign);

            // Check if target is reached
            if (campaign.Target.HasValue && campaign.Achieved >= campaign.Target)
            {
                await UpdateCampaignStatusAsync(campaignId, CampaignStatus.Completed);
            }
   
            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Campaign money incremented successfully."
            };
        }

        public async Task<ServiceResponse<bool>> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status)
        {
            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
            if (campaign == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Campaign with ID {campaignId} not found."
                };
            }

            await _eventDispatcher.DispatchAsync(new CampaignStatusChangedEvent
            {
                Campaign = campaign,
                OldStatus = campaign.Status.Value,
                NewStatus = status
            });

            campaign.UpdateStatus(status);
            await _campaignRepository.UpdateCampaignAsync(campaign);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Campaign status updated successfully."
            };
        }

        // ===== Filtering & Querying =====

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetAllCampaignsAsync(PaginationParametersDto parametersDto, bool includeDeleted = false)
        {
            var campaigns = await _campaignRepository.GetAllCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, includeDeleted);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByStatusAsync(PaginationParametersDto parametersDto, CampaignStatus status)
        {
            var campaigns = await _campaignRepository.GetCampaignsByStatusAsync(parametersDto.PageNumber, parametersDto.PageSize, status);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns with status {status} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByTypeAsync(PaginationParametersDto parametersDto, CampaignType type)
        {
            var campaigns = await _campaignRepository.GetCampaignsByTypeAsync(parametersDto.PageNumber, parametersDto.PageSize, type);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns with type {type} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetActiveCampaignsAsync(PaginationParametersDto parametersDto)
        {
            return await GetCampaignsByStatusAsync(parametersDto, CampaignStatus.Active);
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> SearchCampaignsAsync(PaginationParametersDto parametersDto, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllCampaignsAsync(parametersDto);
            }

            var campaigns = await _campaignRepository.SearchCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, searchTerm);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Search results for '{searchTerm}' retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetDeletedCampaignsAsync(PaginationParametersDto parametersDto)
        {
            var campaigns = await _campaignRepository.GetDeletedCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Deleted campaigns retrieved successfully."
            };
        }

        // ===== Advanced Filtering =====

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByTargetRangeAsync(PaginationParametersDto parametersDto, double minTarget, double maxTarget)
        {
            if (minTarget < 0 || maxTarget < minTarget)
            {
                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = false,
                    Message = "Invalid target range."
                };
            }

            var campaigns = await _campaignRepository.GetCampaignsByTargetRangeAsync(parametersDto.PageNumber, parametersDto.PageSize, minTarget, maxTarget);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns with target between {minTarget} and {maxTarget} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByAchievementPercentageAsync(PaginationParametersDto parametersDto, double minPercentage)
        {
            if (minPercentage < 0 || minPercentage > 100)
            {
                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = false,
                    Message = "Percentage must be between 0 and 100."
                };
            }

            var campaigns = await _campaignRepository.GetCampaignsByAchievementPercentageAsync(parametersDto.PageNumber, parametersDto.PageSize, minPercentage);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns with achievement >= {minPercentage}% retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsEndingSoonAsync(PaginationParametersDto parametersDto, double remainingValue = 1000)
        {
            var campaigns = await _campaignRepository.GetCampaignsEndingSoonAsync(parametersDto.PageNumber, parametersDto.PageSize, remainingValue);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns ending soon (remaining <= {remainingValue}) retrieved successfully."
            };
        }

        // ===== Statistics & Analytics =====

        public async Task<ServiceResponse<int>> GetTotalCampaignsCountAsync(bool includeDeleted = false)
        {
            var count = await _campaignRepository.GetTotalCampaignsCountAsync(includeDeleted);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Total campaigns count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetTotalActiveCampaignsCountAsync()
        {
            var count = await _campaignRepository.GetTotalActiveCampaignsCountAsync();

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Active campaigns count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetSoloCampaignsCountAsync()
        {
            var count = await _campaignRepository.GetSoloCampaignsCountAsync();

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Solo campaigns count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetSharedCampaignsCountAsync()
        {
            var count = await _campaignRepository.GetSharedCampaignsCountAsync();

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Shared campaigns count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetTotalMoneyRaisedAsync()
        {
            var total = await _campaignRepository.GetTotalMoneyRaisedAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = "Total money raised retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetAverageAchievementPercentageAsync()
        {
            var average = await _campaignRepository.GetAverageAchievementPercentageAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = average,
                Message = "Average achievement percentage retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<CampaignType, int>>> GetCampaignCountByTypeAsync()
        {
            var counts = await _campaignRepository.GetCampaignCountByTypeAsync();

            return new ServiceResponse<Dictionary<CampaignType, int>>
            {
                Success = true,
                Data = counts,
                Message = "Campaign count by type retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<CampaignStatus, int>>> GetCampaignCountByStatusAsync()
        {
            var counts = await _campaignRepository.GetCampaignCountByStatusAsync();

            return new ServiceResponse<Dictionary<CampaignStatus, int>>
            {
                Success = true,
                Data = counts,
                Message = "Campaign count by status retrieved successfully."
            };
        }

        public async Task<ServiceResponse<CampaignStatisticsDto>> GetCampaignStatisticsAsync()
        {
            var statusCounts = await _campaignRepository.GetCampaignCountByStatusAsync();
            var topCampaignsResult = await GetTopCampaignsByAchievementAsync(1);
            var topDonatedResult = await GetTopCampaignsByDonationsAsync(1);

            var statistics = new CampaignStatisticsDto
            {
                TotalCampaigns = await _campaignRepository.GetTotalCampaignsCountAsync(false),
                ActiveCampaigns = statusCounts.GetValueOrDefault(CampaignStatus.Active, 0),
                CompletedCampaigns = statusCounts.GetValueOrDefault(CampaignStatus.Completed, 0),
                PendingCampaigns = statusCounts.GetValueOrDefault(CampaignStatus.Preparing, 0),
                CancelledCampaigns = statusCounts.GetValueOrDefault(CampaignStatus.Dismissed, 0),
                TotalMoneyRaised = await _campaignRepository.GetTotalMoneyRaisedAsync(),
                AverageAchievementPercentage = await _campaignRepository.GetAverageAchievementPercentageAsync(),
                SoloCampaignsCount = await _campaignRepository.GetSoloCampaignsCountAsync(),
                SharedCampaignsCount = await _campaignRepository.GetSharedCampaignsCountAsync(),
                MostSuccessfulCampaign = topCampaignsResult.Data?.FirstOrDefault(),
                MostDonatedCampaign = topDonatedResult.Data?.FirstOrDefault(),
                StatisticsDate = DateTime.UtcNow
            };

            return new ServiceResponse<CampaignStatisticsDto>
            {
                Success = true,
                Data = statistics,
                Message = "Campaign statistics retrieved successfully."
            };
        }

        // ===== Featured & Trending =====

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetTopCampaignsByAchievementAsync(PaginationParametersDto parametersDto, int limit = 10)
        {
            if (limit <= 0) limit = 10;

            var campaigns = await _campaignRepository.GetTopCampaignsByAchievementAsync(parametersDto.PageNumber, parametersDto.PageSize, limit);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Top {limit} campaigns by achievement retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetTopCampaignsByAchievementAsync(int limit = 10)
        {
            if (limit <= 0) limit = 10;

            const int pageNumber = 1;
            const int pageSize = int.MaxValue;

            var (campaigns, totalCount) = await _campaignRepository
                .GetTopCampaignsByAchievementAsync(pageNumber, pageSize, limit);

            var campaignsDtos = _mapper.MapToResponseDtos(campaigns);

            return new ServiceResponse<IEnumerable<CampaignResponseDto>>
            {
                Success = true,
                Data = campaignsDtos,
                Message = $"Top {limit} campaigns by achievement retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetTopCampaignsByDonationsAsync(PaginationParametersDto parametersDto, int limit = 10)
        {
            if (limit <= 0) limit = 10;

            var campaigns = await _campaignRepository.GetTopCampaignsByDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, limit);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Top {limit} campaigns by donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetTopCampaignsByDonationsAsync(int limit = 10)
        {
            if (limit <= 0) limit = 10;

            const int pageNumber = 1;
            const int pageSize = int.MaxValue;

            var (campaigns, totalCount) = await _campaignRepository
                .GetTopCampaignsByDonationsAsync(pageNumber, pageSize, limit);

            var campaignsDtos = _mapper.MapToResponseDtos(campaigns);

            return new ServiceResponse<IEnumerable<CampaignResponseDto>>
            {
                Success = true,
                Data = campaignsDtos,
                Message = $"Top {limit} campaigns by donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetRecentCampaignsAsync(PaginationParametersDto parametersDto,int days = 30)
        {
            if (days <= 0) days = 30;

            var campaigns = await _campaignRepository.GetRecentCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, days);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns from the last {days} days retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetUrgentCampaignsAsync(PaginationParametersDto parametersDto, double minPercentage = 75)
        {
            if (minPercentage < 0 || minPercentage > 100)
            {
                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = false,
                    Message = "Percentage must be between 0 and 100."
                };
            }

            var campaigns = await _campaignRepository.GetUrgentCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, minPercentage);
            var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

            var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Urgent campaigns (>= {minPercentage}%) retrieved successfully."
            };
        }

        // ===== Deadline Operations =====

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsByDeadlineAsync(PaginationParametersDto parametersDto, DateTime deadlineDate, bool includeDeleted = false)
        {
            try
            {
                var campaigns = await _campaignRepository.GetCampaignsByDeadlineAsync(parametersDto.PageNumber, parametersDto.PageSize, deadlineDate, includeDeleted);
                var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

                var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = true,
                    Message = $"Campaigns with deadline <= {deadlineDate:yyyy-MM-dd} retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to retrieve campaigns: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetExpiredCampaignsAsync(PaginationParametersDto parametersDto)
        {
            try
            {
                var campaigns = await _campaignRepository.GetExpiredCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize);
                var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

                var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = true,
                    Message = "Expired campaigns retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to retrieve expired campaigns: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<PagedResultDto<CampaignResponseDto>>> GetCampaignsExpiringSoonAsync(PaginationParametersDto parametersDto,int daysThreshold = 7)
        {
            try
            {
                if (daysThreshold <= 0) daysThreshold = 7;

                var campaigns = await _campaignRepository.GetCampaignsExpiringSoonAsync(parametersDto.PageNumber, parametersDto.PageSize, daysThreshold);
                var campaignsDtos = _mapper.MapToResponseDtos(campaigns.Data);

                var response = campaigns.ToPagedResult(campaignsDtos, parametersDto);

                // Calculate days remaining for each campaign
                foreach (var dto in campaignsDtos)
                {
                    // You might want to add a DaysRemaining property to CampaignResponseDto
                }

                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = true,
                    Message = $"Campaigns expiring within {daysThreshold} days retrieved successfully.",
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PagedResultDto<CampaignResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to retrieve expiring campaigns: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<CampaignResponseDto>>> GetCampaignsExpiringSoonAsync(int daysThreshold = 7)
        {
            try
            {
                if (daysThreshold <= 0) daysThreshold = 7;

                const int pageSize = int.MaxValue;
                const int pageNumber = 1;

                var (campaigns, totalCount) = await _campaignRepository
                    .GetCampaignsExpiringSoonAsync(pageNumber, pageSize, daysThreshold);

                var campaignsDtos = _mapper.MapToResponseDtos(campaigns);

                return new ServiceResponse<IEnumerable<CampaignResponseDto>>
                {
                    Success = true,
                    Message = $"Campaigns expiring within {daysThreshold} days retrieved successfully.",
                    Data = campaignsDtos
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<CampaignResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to retrieve expiring campaigns: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> ExtendCampaignDeadlineAsync(int campaignId, DateTime newDeadline)
        {
            try
            {
                var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
                if (campaign == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Campaign with ID {campaignId} not found."
                    };
                }

                if (newDeadline <= DateTime.Now)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "New deadline must be in the future."
                    };
                }

                if (campaign.Status == CampaignStatus.Completed)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Cannot extend deadline for a completed campaign."
                    };
                }

                var updatedCampaign = await _campaignRepository.ExtendCampaignDeadlineAsync(campaignId, newDeadline);

                await _eventDispatcher.DispatchAsync(new CampaignDeadlineExtendedEvent
                {
                    Campaign = campaign,
                    NewDeadline = newDeadline
                });

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = $"Campaign deadline extended to {newDeadline:yyyy-MM-dd} successfully.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to extend deadline: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> AutoExpireCampaignsAsync()
        {
            try
            {
                // Use THE ONLY method - the paged one with int.MaxValue
                const int pageSize = int.MaxValue;
                const int pageNumber = 1;

                var (expiredCampaigns, totalCount) = await _campaignRepository
                    .GetExpiredCampaignsAsync(pageNumber, pageSize);

                int expiredCount = 0;

                foreach (var campaign in expiredCampaigns)
                {
                    if (campaign.Status == CampaignStatus.Active)
                    {
                        campaign.UpdateStatus(CampaignStatus.Expired);
                        await _campaignRepository.UpdateCampaignAsync(campaign);
                        expiredCount++;
                    }
                }

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Message = $"{expiredCount} campaigns have been marked as expired.",
                    Data = true
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to auto-expire campaigns: {ex.Message}"
                };
            }
        }

        // ===== Bulk Operations =====

        public async Task<ServiceResponse<int>> BulkUpdateCampaignStatusAsync(CampaignStatus oldStatus, CampaignStatus newStatus)
        {
            var count = await _campaignRepository.BulkUpdateCampaignStatusAsync(oldStatus, newStatus);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"{count} campaigns updated from {oldStatus} to {newStatus}."
            };
        }

        public async Task<ServiceResponse<int>> SoftDeleteExpiredCampaignsAsync(int daysAfterCompletion = 30)
        {
            var count = await _campaignRepository.SoftDeleteExpiredCampaignsAsync(daysAfterCompletion);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"{count} expired campaigns deleted successfully."
            };
        }

        // ===== Private Helpers =====

        private static double CalculatePercentage(double? achieved, double? target)
        {
            if (!achieved.HasValue || !target.HasValue || target.Value == 0)
                return 0;

            return Math.Round((achieved.Value / target.Value) * 100, 2);
        }
    }
}
