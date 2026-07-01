using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheCharityDAL.Entities;

namespace TheCharityDAL.Database
{
    public class TheCharityDbContext : IdentityDbContext<User>
    {
        public TheCharityDbContext(DbContextOptions<TheCharityDbContext> options) : base(options)
        { }
        public TheCharityDbContext() { }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<DonatedItem> DonatedItems { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<ItemImage> ItemImages { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationContactMethod> OrganizationContactMethods { get; set; }
        public DbSet<PaymentInfo> PaymentsInfo { get; set; }
        public DbSet<SharedCampaign> SharedCampaigns { get; set; }
        public DbSet<SoloCampaign> SoloCampaigns { get; set; }
        public DbSet<ScheduledJob> ScheduledJobs { get; set; }
        public DbSet<OrganizationRole> OrganizationRoles { get; set; }
        public DbSet<SharedCampaignInvite> SharedCampaignInvites { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Campaign TPH configuration
            builder.Entity<Campaign>()
                .ToTable("Campaigns")
                .HasDiscriminator<string>("CampaignType")
                .HasValue<SharedCampaign>("Shared")
                .HasValue<SoloCampaign>("Solo");

            // 2. SoloCampaign relationship
            builder.Entity<SoloCampaign>()
                .HasOne(sc => sc.Organization)
                .WithMany(o => o.SoloCampaigns)
                .HasForeignKey(sc => sc.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. SharedCampaign many-to-many
            builder.Entity<SharedCampaign>()
                .HasMany(sc => sc.Organizations)
                .WithMany(o => o.SharedCampaigns)
                .UsingEntity(j => j.ToTable("SharedCampaignOrganizations"));

            // 4. DonatedItem - Attachment relationships
            builder.Entity<DonatedItem>()
                .HasMany(di => di.ItemAttachments)
                .WithOne(a => a.DonatedItem)
                .HasForeignKey(a => a.DonatedItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DonatedItem>()
                .HasMany(di => di.RecipientAttachments)
                .WithOne() // No inverse navigation for RecipientAttachments
                .HasForeignKey(a => a.DonatedItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Organization - AdminUser relationship
            builder.Entity<Organization>()
                .HasOne(o => o.AdminUser)
                .WithMany()  // No inverse navigation property
                .HasForeignKey(o => o.AdminUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent accidental admin deletion

            // 6. OrganizationRole - User relationship
            builder.Entity<OrganizationRole>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 7. SharedCampaign - CreatorOrganization relationship
            builder.Entity<SharedCampaign>()
                .HasOne(sc => sc.CreatorOrganization)
                .WithMany()
                .HasForeignKey(sc => sc.CreatorOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            // 8. SharedCampaignInvite relationships
            builder.Entity<SharedCampaignInvite>()
                .HasOne(i => i.SharedCampaign)
                .WithMany()
                .HasForeignKey(i => i.SharedCampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SharedCampaignInvite>()
                .HasOne(i => i.Organization)
                .WithMany()
                .HasForeignKey(i => i.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SharedCampaignInvite>()
                .HasOne(i => i.InvitedByUser)
                .WithMany()
                .HasForeignKey(i => i.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 9. Soft delete query filters
            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Campaign>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<Organization>().HasQueryFilter(o => !o.IsDeleted);
            builder.Entity<OrganizationRole>().HasQueryFilter(r => !r.IsDeleted);
            builder.Entity<SharedCampaignInvite>().HasQueryFilter(i => !i.IsDeleted);
        }
    }
}
