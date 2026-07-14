using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityBLL.Services.Abstraction.Payment;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation.PaymentGateway
{
    public class PaymentInfoService : IPaymentInfoService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentInfoService> _logger;

        public PaymentInfoService(
            IOrganizationRepository organizationRepository,
            IMapper mapper,
            ILogger<PaymentInfoService> logger)
        {
            _organizationRepository = organizationRepository;
            _mapper = mapper;
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
                throw new KeyNotFoundException($"Organization with ID {organizationId} was not found.");
            }

            var paymentInfo = await _organizationRepository.GetPaymentInfoByOrganizationIdAsync(organizationId);
            if (paymentInfo == null)
            {
                _logger.LogInformation("No payment info found for organization ID {OrganizationId}.", organizationId);
                return null;
            }
            var paymentInfoResponseDto = _mapper.Map<PaymentInfoResponseDto>(paymentInfo);
            paymentInfoResponseDto.OrganizationId = organizationId;
            return new ServiceResponse<PaymentInfoResponseDto?> { Message = "Payment info retrieved successfully.", Success = true, Data = paymentInfoResponseDto };
            
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> GetPaymentInfoByIdAsync(int paymentInfoId)
        {
            _logger.LogInformation("Fetching payment info with ID {PaymentInfoId}.", paymentInfoId);

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} was not found.", paymentInfoId);
                throw new KeyNotFoundException($"PaymentInfo with ID {paymentInfoId} was not found.");
            }
            var paymentInfoResponseDto = _mapper.Map<PaymentInfoResponseDto>(paymentInfo);
            var or = await _organizationRepository.GetOrganizationByPaymentInfoIdAsync(paymentInfoId);

            if (or != null)
                paymentInfoResponseDto.OrganizationId = or.Id;
            else
                paymentInfoResponseDto.OrganizationId = null;
            return new ServiceResponse<PaymentInfoResponseDto?> { Message = "Payment info retrieved successfully.", Success = true, Data = paymentInfoResponseDto };

            
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> CreatePaymentInfoAsync(CreatePaymentInfoDto dto)
        {
            var organization = await _organizationRepository.GetOrganizationByIdAsync(dto.OrganizationId);

            if (organization == null)
                return null;

            var paymentInfo = _mapper.Map<PaymentInfo>(dto);
            var created = await _organizationRepository.AddPaymentInfoAsync(paymentInfo);

            organization.EditPaymentInfoId(created.Id);

            await _organizationRepository.UpdateOrganizationAsync(organization);

            _logger.LogInformation("Payment info created with ID {PaymentInfoId} .",
                created.Id);
            var responseDto = _mapper.Map<PaymentInfoResponseDto>(created);
            responseDto.OrganizationId = dto.OrganizationId;
            return new ServiceResponse<PaymentInfoResponseDto?> { Message = "Payment info created successfully.", Success = true, Data = responseDto };
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto?>> UpdatePaymentInfoAsync(int paymentInfoId, UpdatePaymentInfoDto dto)
        {
            _logger.LogInformation("Updating payment info with ID {PaymentInfoId}.", paymentInfoId);

            var organization = await _organizationRepository.GetOrganizationByIdAsync(dto.OrganizationId);

            if (organization == null)
            {
                _logger.LogInformation("the organization id  is invalid during Updating payment info with ID {PaymentInfoId}.", paymentInfoId);
                return null;
            }

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} was not found.", paymentInfoId);
                throw new KeyNotFoundException($"PaymentInfo with ID {paymentInfoId} was not found.");
            }

            if (paymentInfo.IsDeleted)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} is deleted and cannot be updated.", paymentInfoId);
                throw new InvalidOperationException($"PaymentInfo with ID {paymentInfoId} is deleted and cannot be updated.");
            }

            /// remove paymentinfo id from the old organization
            var OldOrganization = await _organizationRepository.GetOrganizationByPaymentInfoIdAsync(paymentInfoId);
            if (OldOrganization == null)
            {
                var or = await UpdatePaymentInfoIdOfOrganization(paymentInfoId, dto.OrganizationId);
                if (or is null)
                    return null;
            }
            else if (OldOrganization.Id != dto.OrganizationId)
            {
                OldOrganization.EditPaymentInfoId(null);

                await _organizationRepository.UpdateOrganizationAsync(OldOrganization);
                /// add paymentinfo id to the new organization

                var or = await UpdatePaymentInfoIdOfOrganization(paymentInfoId, dto.OrganizationId);
                if (or is null)
                    return null;
            }



            if (!string.IsNullOrWhiteSpace(dto.ApiKey))
                paymentInfo.EditApiKey(dto.ApiKey);

            if (!string.IsNullOrWhiteSpace(dto.IntegrationId))
                paymentInfo.EditIntegrationId(dto.IntegrationId);

            if (!string.IsNullOrWhiteSpace(dto.IframeId))
                paymentInfo.EditIframeId(dto.IframeId);

            if (!string.IsNullOrWhiteSpace(dto.HmacKey))
                paymentInfo.EditHmacKey(dto.HmacKey);

            var updated = await _organizationRepository.UpdatePaymentInfoAsync(paymentInfo);
            var responsedto = _mapper.Map<PaymentInfoResponseDto>(updated);
            responsedto.OrganizationId = dto.OrganizationId;
            _logger.LogInformation("Payment info with ID {PaymentInfoId} updated successfully.", paymentInfoId);
            return new ServiceResponse<PaymentInfoResponseDto?> { Message = "Payment info updated successfully.", Success = true, Data = responsedto };
            
        }
        private async Task<Organization?> UpdatePaymentInfoIdOfOrganization(int paymentInfoId, int organizationId)
        {
            var NewOrganization = await _organizationRepository.GetOrganizationByIdAsync(organizationId);

            if (NewOrganization == null)
                return null;

            NewOrganization.EditPaymentInfoId(paymentInfoId);

            return await _organizationRepository.UpdateOrganizationAsync(NewOrganization);
        }
        public async Task DeletePaymentInfoAsync(int paymentInfoId)
        {
            _logger.LogInformation("Deleting payment info with ID {PaymentInfoId}.", paymentInfoId);

            var paymentInfo = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} was not found.", paymentInfoId);
                throw new KeyNotFoundException($"PaymentInfo with ID {paymentInfoId} was not found.");
            }

            if (paymentInfo.IsDeleted)
            {
                _logger.LogWarning("PaymentInfo with ID {PaymentInfoId} is already deleted.", paymentInfoId);
                throw new InvalidOperationException($"PaymentInfo with ID {paymentInfoId} is already deleted.");
            }

            await _organizationRepository.DeletePaymentInfoAsync(paymentInfoId);

            _logger.LogInformation("Payment info with ID {PaymentInfoId} deleted successfully.", paymentInfoId);
        }

        // PaymentInfoService.cs
        public async Task<ServiceResponse<bool>> RestorePaymentInfoAsync(int paymentInfoId)
        {
            _logger.LogInformation("Restoring payment info with ID {PaymentInfoId}.", paymentInfoId);

            var exists = await _organizationRepository.GetPaymentInfoByIdAsync(paymentInfoId); // not useful here
            if (exists == null)
            {
                _logger.LogInformation("payment info with ID {PaymentInfoId} doesn`t exists.", paymentInfoId);

                return new ServiceResponse<bool> { Data=false,Success=false,Message= $"payment info with ID {paymentInfoId} doesn`t exists." };
            }

            await _organizationRepository.RestorePaymentInfoAsync(paymentInfoId);

            _logger.LogInformation("Payment info with ID {PaymentInfoId} restored successfully.", paymentInfoId);
            return new ServiceResponse<bool> { Data=true,Success=true,Message= $"Payment info with ID {paymentInfoId} restored successfully." };
        }

        // ===== Utilities =====

        public async Task<ServiceResponse<bool>> HasPaymentInfoAsync(int organizationId)
        {
            _logger.LogInformation("Checking if organization ID {OrganizationId} has payment info.", organizationId);

            var organizationExists = await _organizationRepository.OrganizationExistsAsync(organizationId);
            if (!organizationExists)
            {
                _logger.LogWarning("Organization with ID {OrganizationId} was not found.", organizationId);
                throw new KeyNotFoundException($"Organization with ID {organizationId} was not found.");
            }

            bool IsHas= await _organizationRepository.HasPaymentInfoAsync(organizationId);
            return new ServiceResponse<bool> { Message = "Payment info existence check completed successfully.", Success = true, Data = IsHas };
        }

        public async Task<ServiceResponse<bool>> ValidatePaymentInfoAsync(int organizationId)
        {
            _logger.LogInformation("Validating payment info for organization ID {OrganizationId}.", organizationId);

            var organizationExists = await _organizationRepository.OrganizationExistsAsync(organizationId);
            if (!organizationExists)
            {
                _logger.LogWarning("Organization with ID {OrganizationId} was not found.", organizationId);
                throw new KeyNotFoundException($"Organization with ID {organizationId} was not found.");
            }

            var isValid = await _organizationRepository.ValidatePaymentInfoAsync(organizationId);

            _logger.LogInformation("Payment info validation result for organization ID {OrganizationId}: {IsValid}.",
                organizationId, isValid);

            return new ServiceResponse<bool> { Data=isValid,Success=true,Message= $"Payment info validation result for organization ID {organizationId}: {isValid}." };
        }

       
    }
}
