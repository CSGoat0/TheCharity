using Riok.Mapperly.Abstractions;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityDAL.Entities;

namespace TheCharityBLL.Mapper
{
    [Mapper]
    public partial class PaymentInfoMapper
    {
        // ===== Response Mappings =====

        public PaymentInfoResponseDto MapToPaymentInfoResponseDto(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null) return null!;

            return new PaymentInfoResponseDto
            {
                Id = paymentInfo.Id,
                ApiKey = paymentInfo.ApiKey ?? string.Empty,
                IntegrationId = paymentInfo.IntegrationId ?? string.Empty,
                IframeId = paymentInfo.IframeId ?? string.Empty,
                HmacKey = paymentInfo.HmacKey ?? string.Empty,
                RegistrationDate = paymentInfo.RegistrationDate,
                UpdatedOn = paymentInfo.UpdatedOn,
                IsDeleted = paymentInfo.IsDeleted,
            };
        }

        public IEnumerable<PaymentInfoResponseDto> MapToPaymentInfoResponseDtos(IEnumerable<PaymentInfo> paymentInfos)
        {
            if (paymentInfos == null) return Enumerable.Empty<PaymentInfoResponseDto>();

            var result = new List<PaymentInfoResponseDto>();
            foreach (var paymentInfo in paymentInfos)
            {
                result.Add(MapToPaymentInfoResponseDto(paymentInfo));
            }
            return result;
        }

        // ===== Create Mappings =====

        public PaymentInfo MapToPaymentInfo(CreatePaymentInfoDto dto)
        {
            if (dto == null) return null!;

            return new PaymentInfo(
                dto.ApiKey,
                dto.IntegrationId,
                dto.IframeId,
                dto.HmacKey);
        }

        // ===== Update Mappings =====

        public PaymentInfo MapToPaymentInfo(UpdatePaymentInfoDto dto, PaymentInfo existingPaymentInfo)
        {
            if (dto == null) return null!;
            if (existingPaymentInfo == null) return null!;

            if (!string.IsNullOrWhiteSpace(dto.ApiKey))
                existingPaymentInfo.EditApiKey(dto.ApiKey);

            if (!string.IsNullOrWhiteSpace(dto.IntegrationId))
                existingPaymentInfo.EditApiKey(dto.IntegrationId);

            if (!string.IsNullOrWhiteSpace(dto.IframeId))
                existingPaymentInfo.EditApiKey(dto.IframeId);

            if (!string.IsNullOrWhiteSpace(dto.HmacKey))
                existingPaymentInfo.EditApiKey(dto.HmacKey);

            return existingPaymentInfo;
        }
    }
}