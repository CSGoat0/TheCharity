using Microsoft.EntityFrameworkCore;
using TheCharityDAL.Database;
using TheCharityDAL.Entities;
using TheCharityDAL.Enums;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityDAL.Repositories.Implementation
{
    public class CampaignRepository : ICampaignRepository
    {
        private readonly TheCharityDbContext _context;

        public CampaignRepository(TheCharityDbContext context)
        {
            _context = context;
        }

        // ===== CRUD Operations for Abstract Campaign =====
        public async Task<IEnumerable<Campaign>> GetAllCampaignsAsync(bool includeDeleted = false)
        {
            if (includeDeleted)
                return await _context.Campaigns.ToListAsync();

            return await _context.Campaigns
                .Where(c => c.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<Campaign?> GetCampaignByIdAsync(int id)
        {
            return await _context.Campaigns
                .Where(c => c.Id == id && (c.IsDeleted == false))
                .FirstOrDefaultAsync();
        }

        public async Task<Campaign> AddCampaignAsync(Campaign campaign)
        {
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<Campaign> UpdateCampaignAsync(Campaign campaign)
        {
            _context.Entry(campaign).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task DeleteCampaignAsync(int id)
        {
            var campaign = await GetCampaignByIdAsync(id);
            if (campaign != null)
            {
                campaign.Delete();
                await _context.SaveChangesAsync();
            }
        }

        public async Task RestoreCampaignAsync(int id)
        {
            var campaign = await _context.Campaigns
                .IgnoreQueryFilters()
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync();

            if (campaign != null)
            {
                campaign.Restore();
                await _context.SaveChangesAsync();
            }
        }

        // ===== Type-Specific CRUD Operations =====
        public async Task<IEnumerable<SoloCampaign>> GetAllSoloCampaignsAsync()
        {
            return await _context.SoloCampaigns
                .Where(c => c.IsDeleted == false)
                .Include(c => c.Organization)
                .ToListAsync();
        }

        public async Task<SoloCampaign?> GetSoloCampaignByIdAsync(int id)
        {
            return await _context.SoloCampaigns
                .Where(c => c.Id == id && (c.IsDeleted == false))
                .Include(c => c.Organization)
                .FirstOrDefaultAsync();
        }

        public async Task<SoloCampaign> AddSoloCampaignAsync(SoloCampaign campaign)
        {
            _context.SoloCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<SoloCampaign> UpdateSoloCampaignAsync(SoloCampaign campaign)
        {
            _context.Entry(campaign).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<IEnumerable<SharedCampaign>> GetAllSharedCampaignsAsync()
        {
            return await _context.SharedCampaigns
                .Where(c => c.IsDeleted == false)
                .Include(c => c.Organizations)
                .ToListAsync();
        }

        public async Task<SharedCampaign?> GetSharedCampaignByIdAsync(int id)
        {
            return await _context.SharedCampaigns
                .Where(c => c.Id == id && (c.IsDeleted == false))
                .Include(c => c.Organizations)
                .FirstOrDefaultAsync();
        }

        public async Task<SharedCampaign> AddSharedCampaignAsync(SharedCampaign campaign)
        {
            _context.SharedCampaigns.Add(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<SharedCampaign> UpdateSharedCampaignAsync(SharedCampaign campaign)
        {
            _context.Entry(campaign).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return campaign;
        }

        // ===== Filtering & Querying =====
        public async Task<IEnumerable<Campaign>> GetCampaignsByStatusAsync(CampaignStatus status)
        {
            return await _context.Campaigns
                .Where(c => c.Status == status && (c.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsByTypeAsync(CampaignType type)
        {
            return await _context.Campaigns
                .Where(c => c.Type == type && (c.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetActiveCampaignsAsync()
        {
            return await GetCampaignsByStatusAsync(CampaignStatus.Active);
        }

        public async Task<IEnumerable<Campaign>> SearchCampaignsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCampaignsAsync();

            return await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           (c.Title != null && c.Title.Contains(searchTerm)) ||
                           (c.Description != null && c.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetDeletedCampaignsAsync()
        {
            return await _context.Campaigns
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted == true)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoloCampaign>> GetSoloCampaignsByOrganizationIdAsync(int organizationId)
        {
            return await _context.SoloCampaigns
                .Where(c => c.OrganizationId == organizationId &&
                           (c.IsDeleted == false))
                .Include(c => c.Organization)
                .ToListAsync();
        }

        public async Task<IEnumerable<SharedCampaign>> GetSharedCampaignsByOrganizationIdAsync(int organizationId)
        {
            return await _context.SharedCampaigns
                .Where(c => c.Organizations != null &&
                           c.Organizations.Any(o => o.Id == organizationId) &&
                           (c.IsDeleted == false))
                .Include(c => c.Organizations)
                .ToListAsync();
        }

        public async Task<IEnumerable<SoloCampaign>> GetSoloCampaignsByStatusAsync(CampaignStatus status)
        {
            return await _context.SoloCampaigns
                .Where(c => c.Status == status && (c.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<IEnumerable<SharedCampaign>> GetSharedCampaignsByStatusAsync(CampaignStatus status)
        {
            return await _context.SharedCampaigns
                .Where(c => c.Status == status && (c.IsDeleted == false))
                .ToListAsync();
        }

        // ===== SharedCampaign Specific Operations =====
        public async Task AddOrganizationToSharedCampaignAsync(int sharedCampaignId, Organization organization)
        {
            var campaign = await GetSharedCampaignByIdAsync(sharedCampaignId);
            if (campaign != null)
            {
                campaign.AddOrganization(organization);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveOrganizationFromSharedCampaignAsync(int sharedCampaignId, Organization organization)
        {
            var campaign = await GetSharedCampaignByIdAsync(sharedCampaignId);
            if (campaign != null)
            {
                campaign.RemoveOrganization(organization);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetOrganizationCountForSharedCampaignAsync(int sharedCampaignId)
        {
            var campaign = await GetSharedCampaignByIdAsync(sharedCampaignId);
            return campaign?.Organizations?.Count ?? 0;
        }

        // ===== Campaign Progress Operations =====
        public async Task<Campaign?> UpdateCampaignMoneyAsync(int campaignId, double achievedAmount)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign != null)
            {
                campaign.UpdateMoneyAchieved(achievedAmount);
                await _context.SaveChangesAsync();
            }
            return campaign;
        }

        public async Task<Campaign?> IncrementCampaignMoneyAsync(int campaignId, double amount)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign != null && campaign.Achieved.HasValue)
            {
                var newAmount = (campaign.Achieved.HasValue ? campaign.Achieved : 1).Value + amount;
                campaign.UpdateMoneyAchieved(newAmount);
                await _context.SaveChangesAsync();
            }
            return campaign;
        }

        public async Task<Campaign?> UpdateCampaignStatusAsync(int campaignId, CampaignStatus status)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign != null)
            {
                campaign.UpdateStatus(status);
                await _context.SaveChangesAsync();
            }
            return campaign;
        }

        // ===== Advanced Filtering =====
        public async Task<IEnumerable<Campaign>> GetCampaignsByTargetRangeAsync(double minTarget, double maxTarget)
        {
            return await _context.Campaigns
                .Where(c => c.Target >= minTarget &&
                           c.Target <= maxTarget &&
                           (c.IsDeleted == false))
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsByAchievementPercentageAsync(double minPercentage)
        {
            return await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           c.Target.HasValue && c.Target > 0 &&
                           c.Achieved.HasValue)
                .ToListAsync()
                .ContinueWith(task => task.Result
                    .Where(c => ((c.Achieved.HasValue ? c.Achieved : 1) / (c.Target.HasValue ? c.Target : 1) * 100) >= minPercentage)
                    .AsEnumerable());
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsNearTargetAsync(int percentageThreshold = 90)
        {
            return await GetCampaignsByAchievementPercentageAsync(percentageThreshold);
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsEndingSoonAsync(double remainingValue = 1000)
        {
            return await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           c.Status == CampaignStatus.Active)
                .Where(c => (c.Target - c.Achieved) <= remainingValue)
                .ToListAsync();
        }

        // ===== Statistics & Analytics =====
        public async Task<int> GetTotalCampaignsCountAsync(bool includeDeleted = false)
        {
            if (includeDeleted == false)
                return await _context.Campaigns
                    .Where(c => c.IsDeleted == false)
                    .CountAsync();
            return await _context.Campaigns.CountAsync();
        }

        public async Task<int> GetTotalActiveCampaignsCountAsync()
        {
            return await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           c.Status == CampaignStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetSoloCampaignsCountAsync()
        {
            return await _context.SoloCampaigns
                .Where(c => c.IsDeleted == false)
                .CountAsync();
        }

        public async Task<int> GetSharedCampaignsCountAsync()
        {
            return await _context.SharedCampaigns
                .Where(c => c.IsDeleted == false)
                .CountAsync();
        }

        public async Task<double> GetTotalMoneyRaisedAsync()
        {
            return await _context.Campaigns
                .Where(c => c.IsDeleted == false)
                .SumAsync(c => c.Achieved ?? 0);
        }

        public async Task<double> GetTotalMoneyRaisedBySoloCampaignsAsync()
        {
            return await _context.SoloCampaigns
                .Where(c => c.IsDeleted == false)
                .SumAsync(c => c.Achieved ?? 0);
        }

        public async Task<double> GetTotalMoneyRaisedBySharedCampaignsAsync()
        {
            return await _context.SharedCampaigns
                .Where(c => c.IsDeleted == false)
                .SumAsync(c => c.Achieved ?? 0);
        }

        public async Task<double> GetAverageAchievementPercentageAsync()
        {
            var campaigns = await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           c.Target.HasValue && (c.Target.HasValue ? c.Target : 1).Value > 0 &&
                           c.Achieved.HasValue)
                .ToListAsync();

            if (!campaigns.Any())
                return 0;

            var totalPercentage = campaigns.Average(c =>
                (c.Achieved ?? 1)/ (c.Target ?? 1) * 100);

            return totalPercentage;
        }

        public async Task<Dictionary<CampaignType, int>> GetCampaignCountByTypeAsync()
        {
            var result = await _context.Campaigns
                .Where(c => c.IsDeleted == false)
                .GroupBy(c => c.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(r => r.Type ?? CampaignType.type1, r => r.Count);
        }

        public async Task<Dictionary<CampaignStatus, int>> GetCampaignCountByStatusAsync()
        {
            var result = await _context.Campaigns
                .Where(c => c.IsDeleted == false)
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return result.ToDictionary(r => r.Status ?? CampaignStatus.Active, r => r.Count);
        }

        public async Task<Dictionary<int, int>> GetCampaignCountByOrganizationAsync()
        {
            // For solo campaigns
            var soloCounts = await _context.SoloCampaigns
                .Where(c => c.IsDeleted == false)
                .GroupBy(c => c.OrganizationId)
                .Select(g => new { OrgId = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = soloCounts
                .Where(x => x.OrgId.HasValue)
                .ToDictionary(x => (x.OrgId.HasValue ? x.OrgId : 1).Value, x => x.Count);

            // For shared campaigns (count each organization once per campaign)
            var sharedCampaigns = await _context.SharedCampaigns
                .Where(c => c.IsDeleted == false)
                .Include(c => c.Organizations)
                .ToListAsync();

            foreach (var campaign in sharedCampaigns)
            {
                foreach (var org in campaign.Organizations ?? new List<Organization>())
                {
                    if (result.ContainsKey(org.Id))
                        result[org.Id]++;
                    else
                        result[org.Id] = 1;
                }
            }

            return result;
        }

        // ===== Featured & Trending =====
        public async Task<IEnumerable<Campaign>> GetTopCampaignsByAchievementAsync(int limit = 10)
        {
            return await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           c.Target.HasValue && (c.Target ?? 1) > 0 &&
                           c.Achieved.HasValue)
                .OrderByDescending(c => (c.Achieved ?? 1) / (c.Target ?? 1))
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetTopCampaignsByDonationsAsync(int limit = 10)
        {
            return await _context.Campaigns
                .Where(c => c.IsDeleted == false)
                .OrderByDescending(c => c.Achieved ?? 0)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetRecentCampaignsAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);

            return await _context.Campaigns
                .Where(c => (c.IsDeleted == false) &&
                           c.RegistrationDate >= cutoffDate)
                .OrderByDescending(c => c.RegistrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetUrgentCampaignsAsync(double minPercentage = 75)
        {
            var campaigns = await GetCampaignsByAchievementPercentageAsync(minPercentage);
            return campaigns
                .Where(c => c.Status == CampaignStatus.Active)
                .OrderByDescending(c => ((c.Achieved ?? 1) / (c.Target ?? 1)))
                .ToList();
        }

        // ===== Deadline Operations =====
        public async Task<IEnumerable<Campaign>> GetCampaignsByDeadlineAsync(DateTime deadlineDate, bool includeDeleted = false)
        {
            var query = _context.Campaigns.Where(c => c.Deadline <= deadlineDate);

            if (!includeDeleted)
            {
                query = query.Where(c => c.IsDeleted == false);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetExpiredCampaignsAsync()
        {
            return await _context.Campaigns
                .Where(c => c.Deadline < DateTime.Now && c.IsDeleted == false)
                .ToListAsync();
        }

        public async Task<IEnumerable<Campaign>> GetCampaignsExpiringSoonAsync(int daysThreshold = 7)
        {
            var thresholdDate = DateTime.Now.AddDays(daysThreshold);

            return await _context.Campaigns
                .Where(c => c.Deadline <= thresholdDate
                            && c.Deadline > DateTime.Now
                            && c.IsDeleted == false
                            && c.Status == CampaignStatus.Active)
                .ToListAsync();
        }

        public async Task<Campaign?> ExtendCampaignDeadlineAsync(int campaignId, DateTime newDeadline)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign != null && newDeadline > DateTime.Now)
            {
                campaign.ExtendDeadline(newDeadline);
                await _context.SaveChangesAsync();
            }
            return campaign;
        }

        // ===== Bulk Operations =====
        public async Task<int> BulkUpdateCampaignStatusAsync(CampaignStatus oldStatus, CampaignStatus newStatus)
        {
            var campaigns = await _context.Campaigns
                .Where(c => c.Status == oldStatus &&
                           (c.IsDeleted == false))
                .ToListAsync();

            foreach (var campaign in campaigns)
            {
                campaign.UpdateStatus(newStatus);
            }

            await _context.SaveChangesAsync();
            return campaigns.Count;
        }

        public async Task<int> SoftDeleteExpiredCampaignsAsync(int daysAfterCompletion = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-daysAfterCompletion);

            var campaigns = await _context.Campaigns
                .Where(c => (c.Status == CampaignStatus.Completed &&
                           c.CompletionDate < cutoffDate &&
                           c.IsDeleted == false
                )).ToListAsync();

            foreach (var campaign in campaigns)
            {
                campaign.Delete();
            }

            await _context.SaveChangesAsync();
            return campaigns.Count;
        }

        // ===== Utility Methods =====
        public async Task<bool> CampaignExistsAsync(int id)
        {
            return await _context.Campaigns.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> IsCampaignActiveAsync(int id)
        {
            var campaign = await GetCampaignByIdAsync(id);
            return campaign?.Status == CampaignStatus.Active;
        }

        public async Task<double> GetCampaignAchievementPercentageAsync(int id)
        {
            var campaign = await GetCampaignByIdAsync(id);
            if (campaign == null || !campaign.Target.HasValue ||
                (campaign.Target.HasValue ? campaign.Target : 1).Value == 0 || !campaign.Achieved.HasValue)
                return 0;

            return (double)(campaign.Achieved.HasValue ? campaign.Achieved : 1).Value / (campaign.Target.HasValue ? campaign.Target : 1).Value * 100;
        }

        public async Task<CampaignType?> GetCampaignTypeAsync(int id)
        {
            var campaign = await GetCampaignByIdAsync(id);
            return campaign?.Type;
        }

        // Helper method to get campaign as specific type
        public async Task<T?> GetCampaignAsAsync<T>(int id) where T : Campaign
        {
            if (typeof(T) == typeof(SoloCampaign))
                return await GetSoloCampaignByIdAsync(id) as T;
            else if (typeof(T) == typeof(SharedCampaign))
                return await GetSharedCampaignByIdAsync(id) as T;

            return await GetCampaignByIdAsync(id) as T;
        }

        public async Task<IEnumerable<T>> GetCampaignsAsAsync<T>() where T : Campaign
        {
            if (typeof(T) == typeof(SoloCampaign))
                return (await GetAllSoloCampaignsAsync()).Cast<T>();
            else if (typeof(T) == typeof(SharedCampaign))
                return (await GetAllSharedCampaignsAsync()).Cast<T>();

            return (await GetAllCampaignsAsync()).Cast<T>();
        }

        // Campaign Ownership
        public async Task<bool> IsCampaignOwnedByOrganizationAsync(int campaignId, int organizationId)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign == null) return false;

            if (campaign is SoloCampaign solo)
                return solo.OrganizationId == organizationId;
            else if (campaign is SharedCampaign shared)
                return shared.CreatorOrganizationId == organizationId;

            return false;
        }

        public async Task<int?> GetCampaignCreatorOrganizationIdAsync(int campaignId)
        {
            var campaign = await GetCampaignByIdAsync(campaignId);
            if (campaign == null) return null;

            if (campaign is SoloCampaign solo)
                return solo.OrganizationId;
            else if (campaign is SharedCampaign shared)
                return shared.CreatorOrganizationId;

            return null;
        }

        // Shared Campaign Invites
        public async Task<SharedCampaignInvite> CreateInviteAsync(SharedCampaignInvite invite)
        {
            _context.SharedCampaignInvites.Add(invite);
            await _context.SaveChangesAsync();
            return invite;
        }

        public async Task<SharedCampaignInvite?> GetInviteByIdAsync(int inviteId)
        {
            return await _context.SharedCampaignInvites
                .Where(i => i.Id == inviteId && !i.IsDeleted)
                .Include(i => i.SharedCampaign)
                .Include(i => i.Organization)
                .Include(i => i.InvitedByUser)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SharedCampaignInvite>> GetInvitesForSharedCampaignAsync(int sharedCampaignId)
        {
            return await _context.SharedCampaignInvites
                .Where(i => i.SharedCampaignId == sharedCampaignId && !i.IsDeleted)
                .Include(i => i.Organization)
                .Include(i => i.InvitedByUser)
                .OrderByDescending(i => i.RegistrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SharedCampaignInvite>> GetPendingInvitesForOrganizationAsync(int organizationId)
        {
            return await _context.SharedCampaignInvites
                .Where(i => i.OrganizationId == organizationId &&
                           i.Status == InviteStatus.Pending &&
                           i.ExpiresAt > DateTime.UtcNow &&
                           !i.IsDeleted)
                .Include(i => i.SharedCampaign)
                .Include(i => i.InvitedByUser)
                .OrderByDescending(i => i.RegistrationDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SharedCampaignInvite>> GetInvitesSentByUserAsync(string userId)
        {
            return await _context.SharedCampaignInvites
                .Where(i => i.InvitedByUserId == userId && !i.IsDeleted)
                .Include(i => i.SharedCampaign)
                .Include(i => i.Organization)
                .OrderByDescending(i => i.RegistrationDate)
                .ToListAsync();
        }

        public async Task<SharedCampaignInvite> UpdateInviteStatusAsync(int inviteId, InviteStatus status)
        {
            var invite = await GetInviteByIdAsync(inviteId);
            if (invite == null)
                throw new Exception("Invite not found");

            switch (status)
            {
                case InviteStatus.Accepted:
                    invite.Accept();
                    break;
                case InviteStatus.Rejected:
                    invite.Reject();
                    break;
                case InviteStatus.Expired:
                    invite.Expire();
                    break;
                default:
                    throw new Exception("Invalid status");
            }

            await _context.SaveChangesAsync();
            return invite;
        }

        public async Task<bool> HasPendingInviteAsync(int sharedCampaignId, int organizationId)
        {
            return await _context.SharedCampaignInvites
                .AnyAsync(i => i.SharedCampaignId == sharedCampaignId &&
                              i.OrganizationId == organizationId &&
                              i.Status == InviteStatus.Pending &&
                              i.ExpiresAt > DateTime.UtcNow &&
                              !i.IsDeleted);
        }

        public async Task<int> ExpireOldInvitesAsync(DateTime cutoffDate)
        {
            var invites = await _context.SharedCampaignInvites
                .Where(i => i.Status == InviteStatus.Pending &&
                           i.ExpiresAt < cutoffDate &&
                           !i.IsDeleted)
                .ToListAsync();

            foreach (var invite in invites)
            {
                invite.Expire();
            }

            await _context.SaveChangesAsync();
            return invites.Count;
        }

        public async Task<IEnumerable<SharedCampaignInvite>> GetExpiredInvitesAsync(DateTime cutoffDate)
        {
            return await _context.SharedCampaignInvites
                .Where(i => i.Status == InviteStatus.Pending &&
                           i.ExpiresAt < cutoffDate &&
                           !i.IsDeleted)
                .Include(i => i.SharedCampaign)
                .Include(i => i.Organization)
                .ToListAsync();
        }
    }
}
