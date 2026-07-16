using Microsoft.Extensions.Logging;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.Events.InvitationEvents;
using TheCharityBLL.Extensions;
using TheCharityBLL.Services.Abstraction;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation
{
    public class CampaignInviteService : ICampaignInviteService
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILogger<CampaignInviteService> _logger;

        public CampaignInviteService(
            ICampaignRepository campaignRepository,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            IAuthorizationService authorizationService,
            IEventDispatcher eventDispatcher,
            ILogger<CampaignInviteService> logger)
        {
            _campaignRepository = campaignRepository;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _authorizationService = authorizationService;
            _eventDispatcher = eventDispatcher;
            _logger = logger;
        }

        // ===== Send Invite =====

        public async Task<ServiceResponse<InviteResponseDto>> SendInviteAsync(
            int sharedCampaignId,
            int organizationId,
            string invitedByUserId,
            int expiresInDays = 7)
        {
            try
            {
                // 1. Validate shared campaign exists
                var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(sharedCampaignId);
                if (campaign == null)
                {
                    return new ServiceResponse<InviteResponseDto>
                    {
                        Success = false,
                        Message = $"Shared campaign with ID {sharedCampaignId} not found."
                    };
                }

                // 2. Validate organization exists
                var organization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<InviteResponseDto>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                // 3. Check if organization is already part of the campaign
                if (campaign.Organizations != null && campaign.Organizations.Any(o => o.Id == organizationId))
                {
                    return new ServiceResponse<InviteResponseDto>
                    {
                        Success = false,
                        Message = $"Organization {organization.Name} is already a member of this campaign."
                    };
                }

                // 4. Check if user has permission to send invites
                var canSend = await _authorizationService.CanSendInviteAsync(invitedByUserId, sharedCampaignId);
                if (!canSend)
                {
                    return new ServiceResponse<InviteResponseDto>
                    {
                        Success = false,
                        Message = "You don't have permission to send invites for this campaign."
                    };
                }

                // 5. Check if there's already a pending invite
                var hasPending = await _campaignRepository.HasPendingInviteAsync(sharedCampaignId, organizationId);
                if (hasPending)
                {
                    return new ServiceResponse<InviteResponseDto>
                    {
                        Success = false,
                        Message = $"Organization {organization.Name} already has a pending invite."
                    };
                }

                // 6. Create the invite
                var expiresAt = DateTime.UtcNow.AddDays(expiresInDays);
                var invite = new SharedCampaignInvite(sharedCampaignId, organizationId, invitedByUserId, expiresAt);
                var created = await _campaignRepository.CreateInviteAsync(invite);

                // 7. Get inviter name
                var inviter = await _userRepository.GetUserByIdAsync(invitedByUserId);
                var inviterName = inviter?.FullName ?? inviter?.UserName ?? "Unknown User";
                var invitedOrg = await _organizationRepository.GetOrganizationByIdAsync(organizationId);
                var invitedByUser = await _userRepository.GetUserByIdAsync(invitedByUserId);

                // 8. Dispatch event for notification
                await _eventDispatcher.DispatchAsync(new InviteSentEvent
                {
                    Invite = created,
                    Campaign = campaign!,
                    InvitedOrganization = invitedOrg!,
                    InvitedByUser = invitedByUser!
                });

                var response = MapToInviteResponseDto(created);

                _logger.LogInformation("Invite sent: Campaign {CampaignId} → Organization {OrganizationId} by {UserId}",
                    sharedCampaignId, organizationId, invitedByUserId);

                return new ServiceResponse<InviteResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = $"Invite sent to {organization.Name} successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send invite: Campaign {CampaignId} → Organization {OrganizationId}",
                    sharedCampaignId, organizationId);
                return new ServiceResponse<InviteResponseDto>
                {
                    Success = false,
                    Message = $"Failed to send invite: {ex.Message}"
                };
            }
        }

        // ===== Accept Invite =====

        public async Task<ServiceResponse<bool>> AcceptInviteAsync(int inviteId, string userId)
        {
            try
            {
                // 1. Get the invite
                var invite = await _campaignRepository.GetInviteByIdAsync(inviteId);
                if (invite == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Invite with ID {inviteId} not found."
                    };
                }

                // 2. Check if invite is still pending
                if (invite.Status != InviteStatus.Pending)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Invite is already {invite.Status}. Cannot accept."
                    };
                }

                // 3. Check if invite has expired
                if (invite.ExpiresAt < DateTime.UtcNow)
                {
                    await _campaignRepository.UpdateInviteStatusAsync(inviteId, InviteStatus.Expired);
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Invite has expired. Please request a new invite."
                    };
                }

                // 4. Check if user has permission to accept
                var canAccept = await _authorizationService.CanAcceptInviteAsync(userId, inviteId);
                if (!canAccept)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "You don't have permission to accept this invite."
                    };
                }

                // 5. Get the campaign and organization
                var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(invite.SharedCampaignId);
                var organization = await _organizationRepository.GetOrganizationByIdAsync(invite.OrganizationId);

                if (campaign == null || organization == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Campaign or organization not found."
                    };
                }

                // 6. Check if organization is already a member
                if (campaign.Organizations != null && campaign.Organizations.Any(o => o.Id == organization.Id))
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Organization is already a member of this campaign."
                    };
                }

                // 7. Add organization to the campaign
                campaign.AddOrganization(organization);
                await _campaignRepository.UpdateSharedCampaignAsync(campaign);

                // 8. Update invite status
                await _campaignRepository.UpdateInviteStatusAsync(inviteId, InviteStatus.Accepted);

                // 9. Get all related entities for the event
                var invitedOrg = await _organizationRepository.GetOrganizationByIdAsync(invite.OrganizationId);
                var acceptedByUser = await _userRepository.GetUserByIdAsync(userId);

                // 10. Dispatch InviteAcceptedEvent
                await _eventDispatcher.DispatchAsync(new InviteAcceptedEvent
                {
                    Invite = invite,
                    Campaign = campaign!,
                    InvitedOrganization = invitedOrg!,
                    AcceptedByUser = acceptedByUser!
                });

                _logger.LogInformation("Invite {InviteId} accepted: Campaign {CampaignId} → Organization {OrganizationId}",
                    inviteId, invite.SharedCampaignId, invite.OrganizationId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Invite accepted. Organization added to campaign successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to accept invite {InviteId}", inviteId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to accept invite: {ex.Message}"
                };
            }
        }

        // ===== Reject Invite =====

        public async Task<ServiceResponse<bool>> RejectInviteAsync(int inviteId, string userId)
        {
            try
            {
                // 1. Get the invite
                var invite = await _campaignRepository.GetInviteByIdAsync(inviteId);
                if (invite == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Invite with ID {inviteId} not found."
                    };
                }

                // 2. Check if invite is still pending
                if (invite.Status != InviteStatus.Pending)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Invite is already {invite.Status}. Cannot reject."
                    };
                }

                // 3. Check if user has permission to reject
                var canReject = await _authorizationService.CanRejectInviteAsync(userId, inviteId);
                if (!canReject)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "You don't have permission to reject this invite."
                    };
                }

                // 4. Update invite status
                await _campaignRepository.UpdateInviteStatusAsync(inviteId, InviteStatus.Rejected);

                // 5. Get all related entities for the event
                var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(invite!.SharedCampaignId);
                var invitedOrg = await _organizationRepository.GetOrganizationByIdAsync(invite.OrganizationId);
                var rejectedByUser = await _userRepository.GetUserByIdAsync(userId);

                // 6. Dispatch InviteRejectedEvent
                await _eventDispatcher.DispatchAsync(new InviteRejectedEvent
                {
                    Invite = invite,
                    Campaign = campaign!,
                    InvitedOrganization = invitedOrg!,
                    RejectedByUser = rejectedByUser!
                });

                _logger.LogInformation("Invite {InviteId} rejected: Campaign {CampaignId} → Organization {OrganizationId}",
                    inviteId, invite.SharedCampaignId, invite.OrganizationId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Invite rejected successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reject invite {InviteId}", inviteId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to reject invite: {ex.Message}"
                };
            }
        }

        // ===== Get Invites For Campaign (with Pagination) =====

        public async Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetInvitesForCampaignAsync(
            PaginationParametersDto parametersDto,
            int sharedCampaignId)
        {
            try
            {
                var (invites, totalCount) = await _campaignRepository.GetInvitesForSharedCampaignAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    sharedCampaignId);

                var inviteDtos = invites.Select(MapToInviteResponseDto);

                var response = new PagedResultDto<InviteResponseDto>
                {
                    Items = inviteDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = true,
                    Data = response,
                    Message = "Invites retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invites for campaign {CampaignId}", sharedCampaignId);
                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to get invites: {ex.Message}"
                };
            }
        }

        // ===== Get Pending Invites For Organization (with Pagination) =====

        public async Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetPendingInvitesForOrganizationAsync(
            PaginationParametersDto parametersDto,
            int organizationId)
        {
            try
            {
                var (invites, totalCount) = await _campaignRepository.GetPendingInvitesForOrganizationAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    organizationId);

                var inviteDtos = invites.Select(MapToInviteResponseDto);

                var response = new PagedResultDto<InviteResponseDto>
                {
                    Items = inviteDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = true,
                    Data = response,
                    Message = "Pending invites retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get pending invites for organization {OrganizationId}", organizationId);
                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to get pending invites: {ex.Message}"
                };
            }
        }

        // ===== Get Invites Sent By User (with Pagination) =====

        public async Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetInvitesSentByUserAsync(
            PaginationParametersDto parametersDto,
            string userId)
        {
            try
            {
                var (invites, totalCount) = await _campaignRepository.GetInvitesSentByUserAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    userId);

                var inviteDtos = invites.Select(MapToInviteResponseDto);

                var response = new PagedResultDto<InviteResponseDto>
                {
                    Items = inviteDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = true,
                    Data = response,
                    Message = "Sent invites retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get invites sent by user {UserId}", userId);
                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to get sent invites: {ex.Message}"
                };
            }
        }

        // ===== Has Pending Invite =====

        public async Task<ServiceResponse<bool>> HasPendingInviteAsync(int sharedCampaignId, int organizationId)
        {
            try
            {
                var hasPending = await _campaignRepository.HasPendingInviteAsync(sharedCampaignId, organizationId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = hasPending,
                    Message = hasPending ? "Pending invite exists." : "No pending invite."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check pending invite for campaign {CampaignId} and organization {OrganizationId}",
                    sharedCampaignId, organizationId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Failed to check pending invite: {ex.Message}"
                };
            }
        }

        // ===== Get Expired Invites (with Pagination) =====

        public async Task<ServiceResponse<PagedResultDto<InviteResponseDto>>> GetExpiredInvitesAsync(
            PaginationParametersDto parametersDto,
            DateTime cutoffDate)
        {
            try
            {
                var (invites, totalCount) = await _campaignRepository.GetExpiredInvitesAsync(
                    parametersDto.PageNumber,
                    parametersDto.PageSize,
                    cutoffDate);

                var inviteDtos = invites.Select(MapToInviteResponseDto);

                var response = new PagedResultDto<InviteResponseDto>
                {
                    Items = inviteDtos,
                    TotalCount = totalCount,
                    PageNumber = parametersDto.PageNumber,
                    PageSize = parametersDto.PageSize
                };

                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = true,
                    Data = response,
                    Message = "Expired invites retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get expired invites before {CutoffDate}", cutoffDate);
                return new ServiceResponse<PagedResultDto<InviteResponseDto>>
                {
                    Success = false,
                    Message = $"Failed to get expired invites: {ex.Message}"
                };
            }
        }

        // ===== Private Helpers =====

        private InviteResponseDto MapToInviteResponseDto(SharedCampaignInvite invite)
        {
            return new InviteResponseDto
            {
                Id = invite.Id,
                SharedCampaignId = invite.SharedCampaignId,
                CampaignTitle = invite.SharedCampaign?.Title,
                OrganizationId = invite.OrganizationId,
                OrganizationName = invite.Organization?.Name,
                InvitedByUserName = invite.InvitedByUser?.FullName ?? invite.InvitedByUser?.UserName ?? "Unknown",
                Status = invite.Status,
                RespondedAt = invite.RespondedAt,
                ExpiresAt = invite.ExpiresAt,
                RegistrationDate = invite.RegistrationDate
            };
        }
    }
}