using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;

namespace TheCharityBLL.Services.Abstraction.Payment
{
    public interface IPaymentInfoService
    {
        Task<ServiceResponse<PaymentInfoResponseDto?>> GetPaymentInfoByOrganizationIdAsync(int organizationId);
        Task<ServiceResponse<PaymentInfoResponseDto?>> GetPaymentInfoByIdAsync(int paymentInfoId);
        Task<ServiceResponse<PaymentInfoResponseDto?>> CreatePaymentInfoAsync(CreatePaymentInfoDto dto);
        Task<ServiceResponse<PaymentInfoResponseDto>> UpdatePaymentInfoAsync(int paymentInfoId, UpdatePaymentInfoDto dto);
        Task DeletePaymentInfoAsync(int paymentInfoId);
        Task<ServiceResponse<bool>> RestorePaymentInfoAsync(int paymentInfoId);

        Task<ServiceResponse<bool>> HasPaymentInfoAsync(int organizationId);
        Task<ServiceResponse<bool>> ValidatePaymentInfoAsync(int organizationId);
    }
}
