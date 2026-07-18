using Microsoft.Extensions.Logging;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction.Payment;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation.PaymentGateway
{
    public class PaymentInfoService : IPaymentInfoService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly PaymentInfoMapper _paymentInfoMapper;
        private readonly ILogger<PaymentInfoService> _logger;

        public PaymentInfoService(
            IOrganizationRepository organizationRepository,
            ILogger<PaymentInfoService> logger)
        {
            _organizationRepository = organizationRepository;
            _paymentInfoMapper = new PaymentInfoMapper();
            _logger = logger;
        }

        // ===== Core CRUD =====

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> GetPaymentInfoByOrganizationIdAsync(int organizationId)
        {
            _logger.LogInformation("Fetching payment info for organization ID {OrganizationId}.", organizationId);

            var organizationExists = await _organizationRepository.OrganizationExistsAsync(organizationId);
            if (!organizationExists)
            {
                _logger.LogWarning("Organization with ID {OrganizationId} was not found.", organizationId);
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} was not found."
                };
            }

            var paymentInfo = await _organizationRepository.GetPaymentInfoByOrganizationIdAsync(organizationId);
            if (paymentInfo == null)
            {
                _logger.LogInformation("No payment info found for organization ID {OrganizationId}.", organizationId);
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"No payment info found for organization ID {organizationId}."
                };
            }

            var paymentInfoResponseDto = _paymentInfoMapper.MapToPaymentInfoResponseDto(paymentInfo);
            paymentInfoResponseDto.OrganizationId = organizationId;

            return new ServiceResponse<PaymentInfoResponseDto?>
            {
                Success = true,
                Data = paymentInfoResponseDto,
                Message = "Payment info retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> GetPaymentInfoByIdAsync(int paymentInfoId)
        {
            _logger.LogInformation("Fetching payment info with ID {PaymentInfoId}.", paymentInfoId);

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} was not found.", paymentInfoId);
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"PaymentInfo with ID {paymentInfoId} was not found."
                };
            }

            var paymentInfoResponseDto = _paymentInfoMapper.MapToPaymentInfoResponseDto(paymentInfo);
            var organization = await _organizationRepository.GetOrganizationByPaymentInfoIdAsync(paymentInfoId);

            paymentInfoResponseDto.OrganizationId = organization?.Id;

            return new ServiceResponse<PaymentInfoResponseDto?>
            {
                Success = true,
                Data = paymentInfoResponseDto,
                Message = "Payment info retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> CreatePaymentInfoAsync(CreatePaymentInfoDto dto)
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(dto.OrganizationId);

            if (organization == null)
            {
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"Organization with ID {dto.OrganizationId} was not found."
                };
            }

            var paymentInfo = _paymentInfoMapper.MapToPaymentInfo(dto);
            var created = await _organizationRepository.AddPaymentInfoAsync(paymentInfo);

            organization.EditPaymentInfoId(created.Id);
            await _organizationRepository.UpdateOrganizationAsync(organization);

            _logger.LogInformation("Payment info created with ID {PaymentInfoId}.", created.Id);

            var responseDto = _paymentInfoMapper.MapToPaymentInfoResponseDto(created);
            responseDto.OrganizationId = dto.OrganizationId;

            return new ServiceResponse<PaymentInfoResponseDto?>
            {
                Success = true,
                Data = responseDto,
                Message = "Payment info created successfully."
            };
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> UpdatePaymentInfoAsync(int paymentInfoId, UpdatePaymentInfoDto dto)
        {
            _logger.LogInformation("Updating payment info with ID {PaymentInfoId}.", paymentInfoId);

            var organization = await _organizationRepository.GetOrganizationByIdAsync(dto.OrganizationId);

            if (organization == null)
            {
                _logger.LogInformation("The organization id is invalid during updating payment info with ID {PaymentInfoId}.", paymentInfoId);
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"Organization with ID {dto.OrganizationId} was not found."
                };
            }

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} was not found.", paymentInfoId);
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"PaymentInfo with ID {paymentInfoId} was not found."
                };
            }

            if (paymentInfo.IsDeleted)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} is deleted and cannot be updated.", paymentInfoId);
                return new ServiceResponse<PaymentInfoResponseDto?>
                {
                    Success = false,
                    Message = $"PaymentInfo with ID {paymentInfoId} is deleted and cannot be updated."
                };
            }

            // Remove paymentinfo id from the old organization
            var oldOrganization = await _organizationRepository.GetOrganizationByPaymentInfoIdAsync(paymentInfoId);
            if (oldOrganization == null)
            {
                var updatedOrg = await UpdatePaymentInfoIdOfOrganization(paymentInfoId, dto.OrganizationId);
                if (updatedOrg == null)
                {
                    return new ServiceResponse<PaymentInfoResponseDto?>
                    {
                        Success = false,
                        Message = $"Failed to update organization with payment info ID {paymentInfoId}."
                    };
                }
            }
            else if (oldOrganization.Id != dto.OrganizationId)
            {
                oldOrganization.EditPaymentInfoId(null);
                await _organizationRepository.UpdateOrganizationAsync(oldOrganization);

                // Add paymentinfo id to the new organization
                var updatedOrg = await UpdatePaymentInfoIdOfOrganization(paymentInfoId, dto.OrganizationId);
                if (updatedOrg == null)
                {
                    return new ServiceResponse<PaymentInfoResponseDto?>
                    {
                        Success = false,
                        Message = $"Failed to update organization with payment info ID {paymentInfoId}."
                    };
                }
            }

            // Update the payment info using the mapper
            paymentInfo = _paymentInfoMapper.MapToPaymentInfo(dto, paymentInfo);
            var updated = await _organizationRepository.UpdatePaymentInfoAsync(paymentInfo);

            var responseDto = _paymentInfoMapper.MapToPaymentInfoResponseDto(updated);
            responseDto.OrganizationId = dto.OrganizationId;

            _logger.LogInformation("Payment info with ID {PaymentInfoId} updated successfully.", paymentInfoId);

            return new ServiceResponse<PaymentInfoResponseDto?>
            {
                Success = true,
                Data = responseDto,
                Message = "Payment info updated successfully."
            };
        }

        private async Task<Organization?> UpdatePaymentInfoIdOfOrganization(int paymentInfoId, int organizationId)
        {
            var newOrganization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);

            if (newOrganization == null)
                return null;

            newOrganization.EditPaymentInfoId(paymentInfoId);
            return await _organizationRepository.UpdateOrganizationAsync(newOrganization);
        }

        public async Task<ServiceResponse<bool>> DeletePaymentInfoAsync(int paymentInfoId)
        {
            _logger.LogInformation("Deleting payment info with ID {PaymentInfoId}.", paymentInfoId);

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} was not found.", paymentInfoId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"PaymentInfo with ID {paymentInfoId} was not found."
                };
            }

            if (paymentInfo.IsDeleted)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} is already deleted.", paymentInfoId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"PaymentInfo with ID {paymentInfoId} is already deleted."
                };
            }

            await _organizationRepository.DeletePaymentInfoAsync(paymentInfoId);

            _logger.LogInformation("Payment info with ID {PaymentInfoId} deleted successfully.", paymentInfoId);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Payment info deleted successfully."
            };
        }

        public async Task<ServiceResponse<bool>> RestorePaymentInfoAsync(int paymentInfoId)
        {
            _logger.LogInformation("Restoring payment info with ID {PaymentInfoId}.", paymentInfoId);

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogInformation("Payment info with ID {PaymentInfoId} doesn't exist.", paymentInfoId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = $"Payment info with ID {paymentInfoId} doesn't exist."
                };
            }

            await _organizationRepository.RestorePaymentInfoAsync(paymentInfoId);

            _logger.LogInformation("Payment info with ID {PaymentInfoId} restored successfully.", paymentInfoId);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Payment info with ID {paymentInfoId} restored successfully."
            };
        }

        // ===== Utilities =====

        public async Task<ServiceResponse<bool>> HasPaymentInfoAsync(int organizationId)
        {
            _logger.LogInformation("Checking if organization ID {OrganizationId} has payment info.", organizationId);

            var organizationExists = await _organizationRepository.OrganizationExistsAsync(organizationId);
            if (!organizationExists)
            {
                _logger.LogWarning("Organization with ID {OrganizationId} was not found.", organizationId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} was not found."
                };
            }

            var hasPaymentInfo = await _organizationRepository.HasPaymentInfoAsync(organizationId);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = hasPaymentInfo,
                Message = "Payment info existence check completed successfully."
            };
        }

        public async Task<ServiceResponse<bool>> ValidatePaymentInfoAsync(int organizationId)
        {
            _logger.LogInformation("Validating payment info for organization ID {OrganizationId}.", organizationId);

            var organizationExists = await _organizationRepository.OrganizationExistsAsync(organizationId);
            if (!organizationExists)
            {
                _logger.LogWarning("Organization with ID {OrganizationId} was not found.", organizationId);
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} was not found."
                };
            }

            var isValid = await _organizationRepository.ValidatePaymentInfoAsync(organizationId);

            _logger.LogInformation("Payment info validation result for organization ID {OrganizationId}: {IsValid}.",
                organizationId, isValid);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = isValid,
                Message = $"Payment info validation result for organization ID {organizationId}: {isValid}."
            };
        }
    }
}