using Riok.Mapperly.Abstractions;
using TheCharityBLL.DTOs.CampaignDTOs;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;

namespace TheCharityBLL.Mapper
{
    [Mapper]
    public partial class CampaignMapper
    {
        // ===== Response Mappings =====

        public CampaignResponseDto MapToResponseDto(Campaign campaign)
        {
            if (campaign == null) return null!;

            return new CampaignResponseDto
            {
                Id = campaign.Id,
                OrganizationId = campaign.OrganizationId ?? 0,
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
                Deadline = campaign.Deadline,
                DaysRemaining = campaign.Deadline.HasValue
                    ? (int)Math.Ceiling((campaign.Deadline.Value - DateTime.Now).TotalDays)
                    : null
            };
        }

        public IEnumerable<CampaignResponseDto> MapToResponseDtos(IEnumerable<Campaign> campaigns)
        {
            var result = new List<CampaignResponseDto>();
            foreach (var campaign in campaigns)
            {
                result.Add(MapToResponseDto(campaign));
            }
            return result;
        }

        public SoloCampaignResponseDto MapToSoloResponseDto(SoloCampaign campaign)
        {
            if (campaign == null) return null!;

            return new SoloCampaignResponseDto
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
                OrganizationId = campaign.OrganizationId,
                OrganizationName = campaign.Organization?.Name ?? string.Empty
            };
        }

        public IEnumerable<SoloCampaignResponseDto> MapToSoloResponseDtos(IEnumerable<SoloCampaign> campaigns)
        {
            var result = new List<SoloCampaignResponseDto>();
            foreach (var campaign in campaigns)
            {
                result.Add(MapToSoloResponseDto(campaign));
            }
            return result;
        }

        public SharedCampaignResponseDto MapToSharedResponseDto(SharedCampaign campaign)
        {
            if (campaign == null) return null!;

            return new SharedCampaignResponseDto
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
                Deadline = campaign.Deadline,
                DaysRemaining = campaign.Deadline.HasValue
                    ? (int)Math.Ceiling((campaign.Deadline.Value - DateTime.Now).TotalDays)
                    : null,
                CreatorOrganizationId = campaign.CreatorOrganizationId,
                CreatorOrganizationName = campaign.CreatorOrganization?.Name ?? string.Empty,
                Organizations = campaign.Organizations?.Select(o => new OrganizationBasicDto
                {
                    Id = o.Id,
                    Name = o.Name
                }).ToList() ?? new List<OrganizationBasicDto>(),
                OrganizationsCount = campaign.Organizations?.Count ?? 0
            };
        }

        public IEnumerable<SharedCampaignResponseDto> MapToSharedResponseDtos(IEnumerable<SharedCampaign> campaigns)
        {
            var result = new List<SharedCampaignResponseDto>();
            foreach (var campaign in campaigns)
            {
                result.Add(MapToSharedResponseDto(campaign));
            }
            return result;
        }

        // ===== Create Mappings (using constructors) =====

        public SoloCampaign MapToSoloEntity(CreateSoloCampaignDto dto)
        {
            if (dto == null) return null!;

            return new SoloCampaign(
                title: dto.Title,
                description: dto.Description,
                imgPath: dto.ImgPath,
                deadline: dto.Deadline ?? DateTime.Now.AddMonths(1),
                target: (int?)(dto.Target ?? 100),
                achieved: 0,
                status: CampaignStatus.Active,
                type: CampaignType.type1,
                organizationId: dto.OrganizationId
            );
        }

        public SharedCampaign MapToSharedEntity(CreateSharedCampaignDto dto)
        {
            if (dto == null) return null!;

            return new SharedCampaign(
                title: dto.Title,
                description: dto.Description,
                imgPath: dto.ImgPath,
                deadline: dto.Deadline ?? DateTime.Now.AddMonths(1),
                target: (int?)(dto.Target ?? 100),
                achieved: 0,
                status: CampaignStatus.Active,
                type: CampaignType.type6,
                 creatorOrganizationId: dto.CreatorOrganizationId
            );
        }
    }
}