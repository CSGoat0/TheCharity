using Riok.Mapperly.Abstractions;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityDAL.Entities;

namespace TheCharityBLL.Mapper
{
    [Mapper]
    public partial class UserMapper
    {
        // ===== Response Mappings =====

        public UserResponseDTO MapToUserResponseDto(User user)
        {
            if (user == null) return null!;

            return new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RegistrationDate = user.RegistrationDate,
                Address = user.Address,
                IsDeleted = user.IsDeleted,
                EmailConfirmed = user.EmailConfirmed,
                DeletedOn = user.DeletedOn,
                UpdatedOn = user.UpdatedOn,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                FullName = user.FullName,
                ImgPath = user.ImgPath
            };
        }

        public User MapToUser(UserResponseDTO dto)
        {
            if (dto == null) return null!;

            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                EmailConfirmed = dto.EmailConfirmed,
                PhoneNumberConfirmed = dto.PhoneNumberConfirmed,
                TwoFactorEnabled = dto.TwoFactorEnabled,
                LockoutEnabled = dto.LockoutEnabled,
                LockoutEnd = dto.LockoutEnd,
                AccessFailedCount = dto.AccessFailedCount,
            };
            if (!string.IsNullOrEmpty(dto.Address)) {
                user.EditAddress(dto.Address);
            }
            if (!string.IsNullOrEmpty(dto.FullName)) { 
                user.EditFullName(dto.FullName);
            }
            if (!string.IsNullOrEmpty(dto.ImgPath)) { 
                user.EditImage(dto.ImgPath);
            }
            return user;
        }

        public IEnumerable<UserResponseDTO> MapToUserResponseDtos(IEnumerable<User> users)
        {
            if (users == null) return Enumerable.Empty<UserResponseDTO>();

            var result = new List<UserResponseDTO>();
            foreach (var user in users)
            {
                result.Add(MapToUserResponseDto(user));
            }
            return result;
        }

        public UserResponseDTO MapToUserDetailResponseDto(User user)
        {
            if (user == null) return null!;

            return new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                IsDeleted = user.IsDeleted,
                DeletedOn = user.DeletedOn,
                RegistrationDate = user.RegistrationDate,
                UpdatedOn = user.UpdatedOn,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnabled = user.LockoutEnabled,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount
            };
        }

        public UserResponseDTO MapToUserListResponseDto(User user)
        {
            if (user == null) return null!;

            return new UserResponseDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsDeleted = user.IsDeleted,
                RegistrationDate = user.RegistrationDate,
                EmailConfirmed = user.EmailConfirmed
            };
        }

        public IEnumerable<UserResponseDTO> MapToUserListResponseDtos(IEnumerable<User> users)
        {
            if (users == null) return Enumerable.Empty<UserResponseDTO>();

            var result = new List<UserResponseDTO>();
            foreach (var user in users)
            {
                result.Add(MapToUserListResponseDto(user));
            }
            return result;
        }

        // ===== Create Mappings =====

        public User MapToUser(CreateUserDTO dto)
        {
            if (dto == null) return null!;

            var user = new User
                        {
                            UserName = dto.UserName,
                            Email = dto.Email,
                            PhoneNumber = dto.PhoneNumber,
                        };
            if (!string.IsNullOrEmpty(dto.Address))
                user.EditAddress(dto.Address);
            return user;
        }

        public User MapToUser(UpdateUserDTO dto, User existingUser)
        {
            if (dto == null) return null!;
            if (existingUser == null) return null!;

            if (!string.IsNullOrWhiteSpace(dto.UserName))
                existingUser.EditUsername(dto.UserName);

            if (!string.IsNullOrWhiteSpace(dto.Address))
                existingUser.EditAddress(dto.Address);

            if (!string.IsNullOrWhiteSpace(dto.Email))
                existingUser.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                existingUser.PhoneNumber = dto.PhoneNumber;

            return existingUser;
        }

        // ===== Login Mappings =====

        public User MapToUser(LoginResponseDto dto)
        {
            if (dto == null) return null!;

            return new User
            {
                UserName = dto.UserName
            };
        }

        // ===== Role Mappings =====

        public AssignRoleRequest MapToAssignRoleRequest(string role)
        {
            return new AssignRoleRequest
            {
                Role = role
            };
        }

        // ===== Password Mappings =====

        public ChangePasswordDTO MapToChangePasswordDto(string currentPassword, string newPassword, string confirmPassword)
        {
            return new ChangePasswordDTO
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };
        }

        // ===== Reset Password Mappings =====

        public ResetPasswordResponseDto MapToResetPasswordResponseDto(string email, string token, string password)
        {
            return new ResetPasswordResponseDto
            {
                Email = email,
                Token = token,
                Password = password
            };
        }
    }
}