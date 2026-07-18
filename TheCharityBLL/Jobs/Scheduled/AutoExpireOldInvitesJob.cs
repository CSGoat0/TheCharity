using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.Events.InvitationEvents;
using TheCharityBLL.Jobs.Base;
using TheCharityBLL.Jobs.Context;
using TheCharityBLL.Jobs.Result.Abstraction;
using TheCharityBLL.Jobs.Result.Implementation;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Jobs.Scheduled
{
    public class AutoExpireOldInvitesJob : BaseJob
    {
        private readonly ICampaignRepository _campaignRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IEventDispatcher _eventDispatcher;

        public AutoExpireOldInvitesJob(
            ICampaignRepository campaignRepository,
            IEventDispatcher eventDispatcher,
            IOrganizationRepository organizationRepository)
        {
            _campaignRepository = campaignRepository;
            _eventDispatcher = eventDispatcher;
            _organizationRepository = organizationRepository;
        }

        public override string JobName => "Expire old pending invites";

        public override async Task<IJobResult> ExecuteAsync(JobContext context)
        {
            var cutoffDate = DateTime.UtcNow;
            var expiredCount = 0;

            // Get all expired invites
            var query = await _campaignRepository.GetExpiredInvitesAsync(1, int.MaxValue, cutoffDate);
            var expiredInvites = query.Data;

            if (expiredInvites == null || !expiredInvites.Any())
                return JobResult.Success("No expired invites found");

            foreach (var invite in expiredInvites)
            {
                // Get campaign and organization for the event
                var campaign = await _campaignRepository.GetSharedCampaignByIdAsync(invite.SharedCampaignId);
                var organization = await _organizationRepository.GetOrganizationByIdAsync(invite.OrganizationId);

                // Dispatch InviteExpiredEvent
                await _eventDispatcher.DispatchAsync(new InviteExpiredEvent
                {
                    Invite = invite,
                    Campaign = campaign!,
                    InvitedOrganization = organization!
                });

                expiredCount++;
            }

            return JobResult.Success($"Expired {expiredCount} invites and fired events");
        }
    }
}
