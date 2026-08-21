using TheCharityDAL.Enums;

namespace TheCharityDAL.Entities
{
    public abstract class Campaign
    {
        public int Id { get; private set; }
        public string? Title { get; private set; }
        public string? Description { get; private set; }
        public string? ImgPath { get; private set; }
        public double? Target { get; private set; } = 100;
        public double? Achieved { get; private set; } = 0;
        public CampaignStatus? Status { get; private set; } = CampaignStatus.Active;
        public virtual int? OrganizationId { get; protected set; }
        public bool IsDeleted { get; private set; } = false;
        public DateTime? DeletedOn { get; private set; } = null;
        public DateTime? RegistrationDate { get; private set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; private set; } = null;
        public DateTime? Deadline { get; private set; } = DateTime.Now.AddMonths(1);
        public DateTime? CompletionDate { get; private set; } = null;
        public Campaign(string? title, string? description, string? imgPath, int? target, int? achieved, CampaignStatus? status, DateTime deadline)
        {
            this.Title = title;
            this.Description = description;
            this.ImgPath = imgPath;
            this.Target = target;
            this.Achieved = achieved;
            this.Status = status;
            this.Deadline = deadline;
        }
        protected Campaign() { }
        public void EditTitle(string? title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                this.Title = title;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void EditDescription(string? description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                this.Description = description;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void EditImage(string? imgPath)
        {
            if (!string.IsNullOrEmpty(imgPath))
            {
                this.ImgPath = imgPath;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void EditTarget(double? target)
        {
            if (target.HasValue)
            {
                this.Target = target;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void UpdateMoneyAchieved(double? achieved)
        {
            if (achieved.HasValue)
            {
                this.Achieved = achieved;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void UpdateStatus(CampaignStatus? status)
        {
            if (status.HasValue)
            {
                this.Status = status;
                this.UpdatedOn = DateTime.Now;
                if (status == CampaignStatus.Completed)
                    this.CompletionDate = DateTime.Now;
                else this.CompletionDate = null;
            }
        }
        public void ExtendDeadline(DateTime newCompletionDate)
        {
            if (newCompletionDate > DateTime.Now)
            {
                this.Deadline = newCompletionDate;
                this.UpdatedOn = DateTime.Now;
                this.UpdateStatus(CampaignStatus.Active);
            }
        }
        public void Delete()
        {
            this.IsDeleted = true;
            this.DeletedOn = DateTime.Now;
        }
        public void Restore()
        {
            this.IsDeleted = false;
            this.UpdatedOn = DateTime.Now;
        }
    }
}
