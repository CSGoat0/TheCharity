using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace TheCharityDAL.Entities
{
    public class User : IdentityUser
    {
        public string? ImgPath { get; private set; }
        public bool IsDeleted { get; private set; } = false;
        public string? FullName { get; private set; } = "User";
        public DateTime? DeletedOn { get; private set; }
        public DateTime? RegistrationDate { get; private set; } = DateTime.Now;
        public DateTime? UpdatedOn { get; private set; }
        public string? Address { get; private set; }
      
        public User() { }
        public User(string? userName,string? FullName, string? email, string? imgPath,string? Phone,string? Address )
        {
            this.UserName = userName;
            this.Email = email;
            this.ImgPath = imgPath;
            this.FullName = FullName;
            this.PhoneNumber = Phone;
            this.Address = Address;
           
        }
        public void EditUsername(string? userName)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                this.FullName = userName;
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
        public void EditAddress(string? address)
        {
            if (!string.IsNullOrEmpty(address))
            {
                this.Address = address;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void EditFullName(string? fullName) {
            if (!string.IsNullOrEmpty(fullName))
            {
                this.FullName = fullName;
                this.UpdatedOn = DateTime.Now;
            }
        }
        public void Delete()
        {
            this.IsDeleted = true;
            DeletedOn = DateTime.Now;
        }
        public void Restore()
        {
            this.IsDeleted = false;
            UpdatedOn = DateTime.Now;
        }
    }
}
