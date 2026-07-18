namespace TheCharityBLL.DTOs.UserDTOs
{
    public class UpdateUserDTO
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public long StorageOwned { get; set; } = 2000 * 1024 * 1024; // in bytes
        public DateTime? LastStorageUpdate { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }
}
