namespace TheCharityBLL.DTOs.UserDTOs
{
    public class UserResponseDTO
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public long StorageOwned { get; set; } = 2000 * 1024 * 1024; // in bytes
        public DateTime? LastStorageUpdate { get; set; }
        public string? Address { get; set; }
        public bool IsDeleted { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime? DeletedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public int AccessFailedCount { get; set; }
        public string? FullName { get; set; }
        public string? ImgPath { get; set; }
    }
}
