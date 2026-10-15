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

        // ===== Create Mappings =====

        public User MapToUser(CreateUserDTO dto)
        {
            if (dto == null) return null!;

            var user = new User(dto.UserName, dto.FullName, dto.Email, dto.ImgPath, dto.PhoneNumber, dto.Address);
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

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                existingUser.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                existingUser.EditFullName(dto.FullName);

            if (!string.IsNullOrWhiteSpace(dto.ImgPath))
                existingUser.EditImage(dto.ImgPath);

            return existingUser;
        }
    }
}