using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Extensions;
﻿using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.DonationEvents;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction.MoneyDonation;
using TheCharityDAL.Entities;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation.MoneyDonation
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepository _repo;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly DonationMapper _mapper;

        public DonationService(IDonationRepository repo, DonationMapper mapper, IEventDispatcher eventDispatcher)
        {
            _repo = repo;
            _mapper = mapper;
            _eventDispatcher = eventDispatcher;
        }

        // ===== CRUD =====

        public async Task<PagedResultDto<DonationResponseDto>> GetAllDonationsAsync(PaginationParametersDto parametersDto, bool includeDeleted = false)
        {
            var donations = await _repo.GetAllDonationsAsync(parametersDto.PageNumber,parametersDto.PageSize,includeDeleted);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<DonationResponseDto?> GetDonationByIdAsync(int id)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            return donation is null ? null : _mapper.MapToDonationResponseDto(donation);
        }

        public async Task<DonationResponseDto> CreateDonationAsync(CreateDonationDto dto)
        {
            var entity = _mapper.MapToDonation(dto);

            var isValid = await _repo.IsDonationValidAsync(entity);
            if (!isValid)
                throw new InvalidOperationException("Donation data is invalid.");

            if (dto.CampaignId.HasValue && dto.Amount.HasValue)
                await _eventDispatcher.DispatchAsync(new CampaignDonationReceivedEvent
                {
                    CampaignId = dto.CampaignId.Value,
                    Amount = dto.Amount.Value
                });

            var created = await _repo.AddDonationAsync(entity);
            return _mapper.MapToDonationResponseDto(created);
        }

        public async Task<DonationResponseDto?> UpdateDonationAsync(int id, UpdateDonationDto dto)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            if (donation is null) return null;

            donation.EditAmount(dto.Amount);

            if (dto.CampaignId.HasValue)
                donation.EditCampaign(dto.CampaignId.Value);

            var isValid = await _repo.IsDonationValidAsync(donation);
            if (!isValid)
                throw new InvalidOperationException("Updated donation data is invalid.");

            var updated = await _repo.UpdateDonationAsync(donation);
            return _mapper.MapToDonationResponseDto(updated);
        }

        public async Task<bool> DeleteDonationAsync(int id)
        {
            var exists = await _repo.DonationExistsAsync(id);
            if (!exists) return false;

            await _repo.DeleteDonationAsync(id);
            return true;
        }

        public async Task<bool> RestoreDonationAsync(int id)
        {
            var deleted = await _repo.GetDeletedDonationsAsync();
            if (!deleted.Any(d => d.Id == id)) return false;

            await _repo.RestoreDonationAsync(id);
            return true;
        }

        // ===== Filtering & Search =====

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByUserAsync(PaginationParametersDto parametersDto, string userId)
        {
            var donations = await _repo.GetDonationsByUserAsync(parametersDto.PageNumber, parametersDto.PageSize, userId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }
            

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByCampaignAsync(PaginationParametersDto parametersDto, int campaignId)
        {
            var donations = await _repo.GetDonationsByCampaignAsync(parametersDto.PageNumber, parametersDto.PageSize, campaignId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByAmountRangeAsync(PaginationParametersDto parametersDto, double minAmount, double maxAmount)
        {
            var donations = await _repo.GetDonationsByAmountRangeAsync(parametersDto.PageNumber, parametersDto.PageSize, minAmount, maxAmount);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByDateRangeAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate)
        {
            var donations = await _repo.GetDonationsByDateRangeAsync(parametersDto.PageNumber, parametersDto.PageSize, startDate, endDate);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetRecentDonationsAsync(PaginationParametersDto parametersDto, int days = 30)
        {
            var donations = await _repo.GetRecentDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, days);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetDeletedDonationsAsync(PaginationParametersDto parametersDto)
        {
            var donations = await _repo.GetDeletedDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        // ===== Statistics =====

        public Task<double> GetTotalDonationsAmountAsync()
            => _repo.GetTotalDonationsAmountAsync();

        public Task<double> GetTotalDonationsAmountByUserAsync(string userId)
            => _repo.GetTotalDonationsAmountByUserAsync(userId);

        public Task<double> GetTotalDonationsAmountByCampaignAsync(int campaignId)
            => _repo.GetTotalDonationsAmountByCampaignAsync(campaignId);

        public Task<int> GetTotalDonationsCountAsync()
            => _repo.GetTotalDonationsCountAsync();

        public Task<int> GetDonationsCountByUserAsync(string userId)
            => _repo.GetDonationsCountByUserAsync(userId);

        public Task<int> GetDonationsCountByCampaignAsync(int campaignId)
            => _repo.GetDonationsCountByCampaignAsync(campaignId);

        // ===== Advanced Analytics =====

        public Task<double> GetAverageDonationAmountAsync()
            => _repo.GetAverageDonationAmountAsync();

        public Task<double> GetAverageDonationAmountByUserAsync(string userId)
            => _repo.GetAverageDonationAmountByUserAsync(userId);

        public Task<double> GetAverageDonationAmountByCampaignAsync(int campaignId)
            => _repo.GetAverageDonationAmountByCampaignAsync(campaignId);

        public Task<Dictionary<string, double>> GetTopDonorsByAmountAsync(int limit = 10)
            => _repo.GetTopDonorsByAmountAsync(limit);

        public Task<Dictionary<int, double>> GetTopCampaignsByDonationsAsync(int limit = 10)
            => _repo.GetTopCampaignsByDonationsAsync(limit);

        public Task<Dictionary<DateTime, double>> GetDonationsTrendAsync(int days = 30)
            => _repo.GetDonationsTrendAsync(days);

        public Task<Dictionary<string, int>> GetDonationFrequencyByUserAsync()
            => _repo.GetDonationFrequencyByUserAsync();

        // ===== Campaign-Specific =====

        public Task<double> GetCampaignTotalRaisedAsync(int campaignId)
            => _repo.GetCampaignTotalRaisedAsync(campaignId);

        public Task<double> GetCampaignProgressPercentageAsync(int campaignId)
            => _repo.GetCampaignProgressPercentageAsync(campaignId);

        public async Task<PagedResultDto<DonationResponseDto>> GetUsersDonationsOfACampaignAsync(PaginationParametersDto parametersDto, int campaignId)
        {
            var donations = await _repo.GetUsersDonationsOfACampaignAsync(parametersDto.PageNumber, parametersDto.PageSize, campaignId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        // ===== User-Specific =====

        public async Task<PagedResultDto<DonationResponseDto>> GetUserDonationHistoryAsync(PaginationParametersDto parametersDto, string userId)
        {
            var donations = await _repo.GetUserDonationHistoryAsync(parametersDto.PageNumber, parametersDto.PageSize, userId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public Task<DateTime?> GetUserLastDonationDateAsync(string userId)
            => _repo.GetUserLastDonationDateAsync(userId);

        public Task<IEnumerable<int>> GetCampaignsDonatedByUserAsync(string userId)
            => _repo.GetCampaignsDonatedByUserAsync(userId);

        // ===== Bulk Operations =====

        public Task<int> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId)
            => _repo.TransferDonationsToCampaignAsync(fromCampaignId, toCampaignId);

        public Task<int> DeleteOldDonationsAsync(int daysOld = 365)
            => _repo.DeleteOldDonationsAsync(daysOld);

        // ===== Validation =====

        public Task<bool> DonationExistsAsync(int id)
            => _repo.DonationExistsAsync(id);

        public Task<bool> HasUserDonatedToCampaignAsync(string userId, int campaignId)
            => _repo.HasUserDonatedToCampaignAsync(userId, campaignId);

        // ===== Eager Loading =====

        public async Task<DonationResponseDto?> GetDonationWithDetailsAsync(int id)
        {
            var donation = await _repo.GetDonationWithDetailsAsync(id);
            return donation is null ? null : _mapper.MapToDonationResponseDto(donation);
        }

        // ===== Dashboard & Reporting =====

        public async Task<PagedResultDto<DonationResponseDto>> GetLatestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10)
        {
            var donations = await _repo.GetLatestDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, limit);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetLargestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10)
        {
            var donations = await _repo.GetLargestDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, limit);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }   

        public Task<Dictionary<int, int>> GetDonationsPerCampaignCountAsync()
            => _repo.GetDonationsPerCampaignCountAsync();

        public Task<Dictionary<string, int>> GetDonationsPerUserCountAsync()
            => _repo.GetDonationsPerUserCountAsync();

        public Task<double> GetTodayDonationsTotalAsync()
            => _repo.GetTodayDonationsTotalAsync();

        public Task<double> GetThisWeekDonationsTotalAsync()
            => _repo.GetThisWeekDonationsTotalAsync();

        public Task<double> GetThisMonthDonationsTotalAsync()
            => _repo.GetThisMonthDonationsTotalAsync();

        // ===== Financial Reporting =====

        public Task<Dictionary<string, double>> GetMonthlyDonationsReportAsync(int year)
            => _repo.GetMonthlyDonationsReportAsync(year);

        public Task<Dictionary<string, double>> GetQuarterlyDonationsReportAsync(int year)
            => _repo.GetQuarterlyDonationsReportAsync(year);

        public Task<Dictionary<string, double>> GetYearlyDonationsReportAsync(int yearsBack = 5)
            => _repo.GetYearlyDonationsReportAsync(yearsBack);

        public Task<Dictionary<string, double>> GetDonationsByTimeOfDayAsync()
            => _repo.GetDonationsByTimeOfDayAsync();

        public Task<Dictionary<string, double>> GetDonationsByDayOfWeekAsync()
            => _repo.GetDonationsByDayOfWeekAsync();

        // ===== Campaign Performance =====

        public Task<Dictionary<DateTime, double>> GetCampaignDonationTimelineAsync(int campaignId)
            => _repo.GetCampaignDonationTimelineAsync(campaignId);

        // ===== User Engagement =====

        public async Task<PagedResultDto<DonationResponseDto>> GetRecurringDonorsAsync(PaginationParametersDto parametersDto, int minDonations = 3)
        {
            var donors = await _repo.GetRecurringDonorsAsync(parametersDto.PageNumber, parametersDto.PageSize, minDonations);
            var donorsDtos = _mapper.MapToDonationResponseDtos(donors.Data);
            return donors.ToPagedResult(donorsDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetFirstTimeDonorsAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate)
        {
            var donors = await _repo.GetFirstTimeDonorsAsync(parametersDto.PageNumber, parametersDto.PageSize, startDate, endDate);
            var donorsDtos = _mapper.MapToDonationResponseDtos(donors.Data);
            return donors.ToPagedResult(donorsDtos, parametersDto);
        }

        public Task<Dictionary<string, double>> GetUserLifetimeValueAsync()
            => _repo.GetUserLifetimeValueAsync();

        public Task<IEnumerable<string>> GetLoyalDonorsAsync(double minTotalAmount = 1000, int minDonations = 5)
            => _repo.GetLoyalDonorsAsync(minTotalAmount, minDonations);

        // ===== Search & Filter Combinations =====

        public async Task<PagedResultDto<DonationResponseDto>> SearchDonationsByUserAndCampaignAsync(PaginationParametersDto parametersDto,string userId, int campaignId)
        {
            var donations = await _repo.SearchDonationsByUserAndCampaignAsync(parametersDto.PageNumber, parametersDto.PageSize, userId, campaignId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);    
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByMultipleUsersAsync(PaginationParametersDto parametersDto,IEnumerable<string> userIds)
        {
            var donations = await _repo.GetDonationsByMultipleUsersAsync(parametersDto.PageNumber, parametersDto.PageSize, userIds);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByMultipleCampaignsAsync(PaginationParametersDto parametersDto, IEnumerable<int> campaignIds)
        {
            var donations = await _repo.GetDonationsByMultipleCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, campaignIds);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        public async Task<PagedResultDto<DonationResponseDto>> GetDonationsByAmountAndDateAsync(PaginationParametersDto parametersDto, double minAmount, DateTime startDate)
        {
            var donations = await _repo.GetDonationsByAmountAndDateAsync(parametersDto.PageNumber, parametersDto.PageSize, minAmount, startDate);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        // ===== Audit =====

        public async Task<PagedResultDto<DonationResponseDto>> GetSuspiciousDonationsAsync(PaginationParametersDto parametersDto, double amountThreshold = 10000)
        {
            var donations = await _repo.GetSuspiciousDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, amountThreshold);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);
            return donations.ToPagedResult(donationDtos, parametersDto);
        }

        // ===== Export =====

        public Task<int> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate)
            => _repo.GetDonationRecordCountForPeriodAsync(startDate, endDate);
    }
}
