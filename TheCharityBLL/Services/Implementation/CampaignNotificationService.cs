using Microsoft.Extensions.Logging;
using TheCharityBLL.Services.Abstraction;
using TheCharityBLL.Services.Enums;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation
{
    public class CampaignNotificationService : ICampaignNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICampaignRepository _campaignRepository;
        private readonly ILogger<CampaignNotificationService> _logger;

        public CampaignNotificationService(
            IEmailService emailService,
            IOrganizationRepository organizationRepository,
            IUserRepository userRepository,
            ICampaignRepository campaignRepository,
            ILogger<CampaignNotificationService> logger)
        {
            _emailService = emailService;
            _organizationRepository = organizationRepository;
            _userRepository = userRepository;
            _campaignRepository = campaignRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all email recipients for a campaign
        /// </summary>
        private async Task<IEnumerable<string>> GetCampaignRecipientsAsync(int campaignId)
        {
            var recipients = new List<string>();

            // 1. Get the campaign to find its organization
            var campaign = await _campaignRepository.GetCampaignByIdAsync(campaignId);
            if (campaign == null) return recipients;

            int? organizationId = null;
            if (campaign is SoloCampaign solo)
                organizationId = solo.OrganizationId;
            else if (campaign is SharedCampaign shared)
                organizationId = shared.CreatorOrganizationId;

            if (!organizationId.HasValue) return recipients;

            // 2. Get Organization Admin
            var admin = await _organizationRepository.GetOrganizationAdminAsync(organizationId.Value);
            if (admin != null && !string.IsNullOrEmpty(admin.Email))
            {
                recipients.Add(admin.Email);
            }

            // 3. Get Organization Sub-Admins
            var subAdmins = await _organizationRepository.GetOrganizationSubAdminsAsync(organizationId.Value);
            foreach (var subAdmin in subAdmins)
            {
                if (!string.IsNullOrEmpty(subAdmin.Email))
                {
                    recipients.Add(subAdmin.Email);
                }
            }

            // 4. Get SuperAdmins (users with SuperAdmin Identity Role)
            var superAdmins = await GetSuperAdminsAsync();
            foreach (var superAdmin in superAdmins)
            {
                if (!string.IsNullOrEmpty(superAdmin.Email))
                {
                    recipients.Add(superAdmin.Email);
                }
            }

            // 5. Remove duplicates
            return recipients.Distinct().ToList();
        }

        /// <summary>
        /// Get all SuperAdmins from Identity
        /// </summary>
        private async Task<IEnumerable<User>> GetSuperAdminsAsync()
        {
            var allUsers = await _userRepository.GetAllUsersAsync();
            var superAdmins = new List<User>();

            if (allUsers == null) return superAdmins;

            foreach (var user in allUsers)
            {
                var isSuperAdmin = await _userRepository.IsSuperAdminAsync(user.Id);
                if (isSuperAdmin)
                {
                    superAdmins.Add(user);
                }
            }

            return superAdmins;
        }

        /// <summary>
        /// Send notification to all stakeholders of a campaign
        /// </summary>
        public async Task SendCampaignNotificationAsync(
            int campaignId,
            string subject,
            string message,
            NotificationType type)
        {
            try
            {
                var recipients = await GetCampaignRecipientsAsync(campaignId);

                if (!recipients.Any())
                {
                    _logger.LogWarning("No email recipients found for campaign {CampaignId}", campaignId);
                    return;
                }

                _logger.LogInformation("Sending '{Type}' notification to {Count} recipients for campaign {CampaignId}",
                    type, recipients.Count(), campaignId);

                // Send to each recipient
                foreach (var email in recipients)
                {
                    await _emailService.SendNotificationAsync(email, subject, message);
                }

                _logger.LogInformation("Successfully sent '{Type}' notification for campaign {CampaignId}", type, campaignId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send campaign notification for campaign {CampaignId}", campaignId);
                throw;
            }
        }

        /// <summary>
        /// Send notification to all SuperAdmins only
        /// </summary>
        public async Task SendSuperAdminNotificationAsync(
            string subject,
            string message,
            NotificationType type)
        {
            try
            {
                var superAdmins = await GetSuperAdminsAsync();
                var emailList = superAdmins
                    .Where(u => !string.IsNullOrEmpty(u.Email))
                    .Select(u => u.Email)
                    .Distinct()
                    .ToList();

                if (!emailList.Any())
                {
                    _logger.LogWarning("No SuperAdmins found to send notification");
                    return;
                }

                _logger.LogInformation("Sending '{Type}' notification to {Count} SuperAdmins", type, emailList.Count);

                foreach (var email in emailList)
                {
                    await _emailService.SendNotificationAsync(email, subject, message);
                }

                _logger.LogInformation("Successfully sent '{Type}' notification to SuperAdmins", type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SuperAdmin notification");
                throw;
            }
        }
    }
}
