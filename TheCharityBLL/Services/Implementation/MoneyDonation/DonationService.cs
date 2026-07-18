using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.DonationDTOs;
using TheCharityBLL.DTOs.PaginationDTOs;
using TheCharityBLL.Extensions;
using TheCharityBLL.Events.Abstraction;
using TheCharityBLL.Events.Events.DonationEvents;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction.MoneyDonation;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation.MoneyDonation
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepository _repo;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly DonationMapper _mapper;

        public DonationService(IDonationRepository repo, IEventDispatcher eventDispatcher)
        {
            _repo = repo;
            _mapper = new DonationMapper();
            _eventDispatcher = eventDispatcher;
        }

        // ===== CRUD =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetAllDonationsAsync(PaginationParametersDto parametersDto, bool includeDeleted = false)
        {
            var donations = await _repo.GetAllDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, includeDeleted);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<DonationResponseDto?>> GetDonationByIdAsync(int id)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            if (donation == null)
            {
                return new ServiceResponse<DonationResponseDto?>
                {
                    Success = false,
                    Message = $"Donation with ID {id} not found."
                };
            }

            var response = _mapper.MapToDonationResponseDto(donation);

            return new ServiceResponse<DonationResponseDto?>
            {
                Success = true,
                Data = response,
                Message = "Donation retrieved successfully."
            };
        }

        public async Task<ServiceResponse<DonationResponseDto>> CreateDonationAsync(CreateDonationDto dto)
        {
            var entity = _mapper.MapToDonation(dto);

            var isValid = await _repo.IsDonationValidAsync(entity);
            if (!isValid)
            {
                return new ServiceResponse<DonationResponseDto>
                {
                    Success = false,
                    Message = "Donation data is invalid."
                };
            }

            if (dto.CampaignId.HasValue && dto.Amount.HasValue)
                await _eventDispatcher.DispatchAsync(new CampaignDonationReceivedEvent
                {
                    CampaignId = dto.CampaignId.Value,
                    Amount = dto.Amount.Value
                });

            var created = await _repo.AddDonationAsync(entity);
            var response = _mapper.MapToDonationResponseDto(created);

            return new ServiceResponse<DonationResponseDto>
            {
                Success = true,
                Data = response,
                Message = "Donation created successfully."
            };
        }

        public async Task<ServiceResponse<DonationResponseDto?>> UpdateDonationAsync(int id, UpdateDonationDto dto)
        {
            var donation = await _repo.GetDonationByIdAsync(id);
            if (donation == null)
            {
                return new ServiceResponse<DonationResponseDto?>
                {
                    Success = false,
                    Message = $"Donation with ID {id} not found."
                };
            }

            donation.EditAmount(dto.Amount);

            if (dto.CampaignId.HasValue)
                donation.EditCampaign(dto.CampaignId.Value);

            var isValid = await _repo.IsDonationValidAsync(donation);
            if (!isValid)
            {
                return new ServiceResponse<DonationResponseDto?>
                {
                    Success = false,
                    Message = "Updated donation data is invalid."
                };
            }

            var updated = await _repo.UpdateDonationAsync(donation);
            var response = _mapper.MapToDonationResponseDto(updated);

            return new ServiceResponse<DonationResponseDto?>
            {
                Success = true,
                Data = response,
                Message = "Donation updated successfully."
            };
        }

        public async Task<ServiceResponse<bool>> DeleteDonationAsync(int id)
        {
            var exists = await _repo.DonationExistsAsync(id);
            if (!exists)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Donation with ID {id} not found."
                };
            }

            await _repo.DeleteDonationAsync(id);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Donation deleted successfully."
            };
        }

        public async Task<ServiceResponse<bool>> RestoreDonationAsync(int id)
        {
            var (deletedDonations, _) = await _repo.GetDeletedDonationsAsync(1, 1);
            if (!deletedDonations.Any(d => d.Id == id))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Donation with ID {id} not found in deleted items."
                };
            }

            await _repo.RestoreDonationAsync(id);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Donation restored successfully."
            };
        }

        // ===== Filtering & Search =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByUserAsync(PaginationParametersDto parametersDto, string userId)
        {
            var donations = await _repo.GetDonationsByUserAsync(parametersDto.PageNumber, parametersDto.PageSize, userId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donations for user {userId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByCampaignAsync(PaginationParametersDto parametersDto, int campaignId)
        {
            var donations = await _repo.GetDonationsByCampaignAsync(parametersDto.PageNumber, parametersDto.PageSize, campaignId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donations for campaign {campaignId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByAmountRangeAsync(PaginationParametersDto parametersDto, double minAmount, double maxAmount)
        {
            var donations = await _repo.GetDonationsByAmountRangeAsync(parametersDto.PageNumber, parametersDto.PageSize, minAmount, maxAmount);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donations between {minAmount} and {maxAmount} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByDateRangeAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate)
        {
            var donations = await _repo.GetDonationsByDateRangeAsync(parametersDto.PageNumber, parametersDto.PageSize, startDate, endDate);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donations from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetRecentDonationsAsync(PaginationParametersDto parametersDto, int days = 30)
        {
            var donations = await _repo.GetRecentDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, days);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Recent donations from the last {days} days retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDeletedDonationsAsync(PaginationParametersDto parametersDto)
        {
            var donations = await _repo.GetDeletedDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Deleted donations retrieved successfully."
            };
        }

        // ===== Statistics =====

        public async Task<ServiceResponse<double>> GetTotalDonationsAmountAsync()
        {
            var total = await _repo.GetTotalDonationsAmountAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = "Total donations amount retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetTotalDonationsAmountByUserAsync(string userId)
        {
            var total = await _repo.GetTotalDonationsAmountByUserAsync(userId);

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = $"Total donations amount for user {userId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetTotalDonationsAmountByCampaignAsync(int campaignId)
        {
            var total = await _repo.GetTotalDonationsAmountByCampaignAsync(campaignId);

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = $"Total donations amount for campaign {campaignId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetTotalDonationsCountAsync()
        {
            var count = await _repo.GetTotalDonationsCountAsync();

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = "Total donations count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetDonationsCountByUserAsync(string userId)
        {
            var count = await _repo.GetDonationsCountByUserAsync(userId);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"Donations count for user {userId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetDonationsCountByCampaignAsync(int campaignId)
        {
            var count = await _repo.GetDonationsCountByCampaignAsync(campaignId);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"Donations count for campaign {campaignId} retrieved successfully."
            };
        }

        // ===== Advanced Analytics =====

        public async Task<ServiceResponse<double>> GetAverageDonationAmountAsync()
        {
            var average = await _repo.GetAverageDonationAmountAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = average,
                Message = "Average donation amount retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetAverageDonationAmountByUserAsync(string userId)
        {
            var average = await _repo.GetAverageDonationAmountByUserAsync(userId);

            return new ServiceResponse<double>
            {
                Success = true,
                Data = average,
                Message = $"Average donation amount for user {userId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetAverageDonationAmountByCampaignAsync(int campaignId)
        {
            var average = await _repo.GetAverageDonationAmountByCampaignAsync(campaignId);

            return new ServiceResponse<double>
            {
                Success = true,
                Data = average,
                Message = $"Average donation amount for campaign {campaignId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, double>>> GetTopDonorsByAmountAsync(int limit = 10)
        {
            var topDonors = await _repo.GetTopDonorsByAmountAsync(limit);

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = topDonors,
                Message = $"Top {limit} donors by amount retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<int, double>>> GetTopCampaignsByDonationsAsync(int limit = 10)
        {
            var topCampaigns = await _repo.GetTopCampaignsByDonationsAsync(limit);

            return new ServiceResponse<Dictionary<int, double>>
            {
                Success = true,
                Data = topCampaigns,
                Message = $"Top {limit} campaigns by donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<DateTime, double>>> GetDonationsTrendAsync(int days = 30)
        {
            var trend = await _repo.GetDonationsTrendAsync(days);

            return new ServiceResponse<Dictionary<DateTime, double>>
            {
                Success = true,
                Data = trend,
                Message = $"Donations trend for the last {days} days retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, int>>> GetDonationFrequencyByUserAsync()
        {
            var frequency = await _repo.GetDonationFrequencyByUserAsync();

            return new ServiceResponse<Dictionary<string, int>>
            {
                Success = true,
                Data = frequency,
                Message = "Donation frequency by user retrieved successfully."
            };
        }

        // ===== Campaign-Specific =====

        public async Task<ServiceResponse<double>> GetCampaignTotalRaisedAsync(int campaignId)
        {
            var total = await _repo.GetCampaignTotalRaisedAsync(campaignId);

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = $"Total raised for campaign {campaignId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetCampaignProgressPercentageAsync(int campaignId)
        {
            var progress = await _repo.GetCampaignProgressPercentageAsync(campaignId);

            return new ServiceResponse<double>
            {
                Success = true,
                Data = progress,
                Message = $"Progress percentage for campaign {campaignId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetUsersDonationsOfACampaignAsync(PaginationParametersDto parametersDto, int campaignId)
        {
            var donations = await _repo.GetUsersDonationsOfACampaignAsync(parametersDto.PageNumber, parametersDto.PageSize, campaignId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Users' donations for campaign {campaignId} retrieved successfully."
            };
        }

        // ===== User-Specific =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetUserDonationHistoryAsync(PaginationParametersDto parametersDto, string userId)
        {
            var donations = await _repo.GetUserDonationHistoryAsync(parametersDto.PageNumber, parametersDto.PageSize, userId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donation history for user {userId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<DateTime?>> GetUserLastDonationDateAsync(string userId)
        {
            var date = await _repo.GetUserLastDonationDateAsync(userId);

            return new ServiceResponse<DateTime?>
            {
                Success = true,
                Data = date,
                Message = date.HasValue ? $"Last donation date for user {userId} retrieved successfully." : $"No donations found for user {userId}."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<int>>> GetCampaignsDonatedByUserAsync(PaginationParametersDto parametersDto, string userId)
        {
            var (campaignIds, totalCount) = await _repo.GetCampaignsDonatedByUserAsync(
                parametersDto.PageNumber,
                parametersDto.PageSize,
                userId);

            var response = new PagedResultDto<int>
            {
                Items = campaignIds,
                TotalCount = totalCount,
                PageNumber = parametersDto.PageNumber,
                PageSize = parametersDto.PageSize
            };

            return new ServiceResponse<PagedResultDto<int>>
            {
                Success = true,
                Data = response,
                Message = $"Campaigns donated by user {userId} retrieved successfully."
            };
        }

        // ===== Bulk Operations =====

        public async Task<ServiceResponse<int>> TransferDonationsToCampaignAsync(int fromCampaignId, int toCampaignId)
        {
            var count = await _repo.TransferDonationsToCampaignAsync(fromCampaignId, toCampaignId);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"{count} donations transferred from campaign {fromCampaignId} to {toCampaignId}."
            };
        }

        public async Task<ServiceResponse<int>> DeleteOldDonationsAsync(int daysOld = 365)
        {
            var count = await _repo.DeleteOldDonationsAsync(daysOld);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"{count} donations older than {daysOld} days deleted successfully."
            };
        }

        // ===== Validation =====

        public async Task<ServiceResponse<bool>> DonationExistsAsync(int id)
        {
            var exists = await _repo.DonationExistsAsync(id);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = exists,
                Message = exists ? $"Donation with ID {id} exists." : $"Donation with ID {id} does not exist."
            };
        }

        public async Task<ServiceResponse<bool>> HasUserDonatedToCampaignAsync(string userId, int campaignId)
        {
            var hasDonated = await _repo.HasUserDonatedToCampaignAsync(userId, campaignId);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = hasDonated,
                Message = hasDonated ? $"User {userId} has donated to campaign {campaignId}." : $"User {userId} has not donated to campaign {campaignId}."
            };
        }

        // ===== Eager Loading =====

        public async Task<ServiceResponse<DonationResponseDto?>> GetDonationWithDetailsAsync(int id)
        {
            var donation = await _repo.GetDonationWithDetailsAsync(id);
            if (donation == null)
            {
                return new ServiceResponse<DonationResponseDto?>
                {
                    Success = false,
                    Message = $"Donation with ID {id} not found."
                };
            }

            var response = _mapper.MapToDonationResponseDto(donation);

            return new ServiceResponse<DonationResponseDto?>
            {
                Success = true,
                Data = response,
                Message = "Donation details retrieved successfully."
            };
        }

        // ===== Dashboard & Reporting =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetLatestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10)
        {
            var donations = await _repo.GetLatestDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, limit);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Latest {limit} donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetLargestDonationsAsync(PaginationParametersDto parametersDto, int limit = 10)
        {
            var donations = await _repo.GetLargestDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, limit);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Largest {limit} donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<int, int>>> GetDonationsPerCampaignCountAsync()
        {
            var counts = await _repo.GetDonationsPerCampaignCountAsync();

            return new ServiceResponse<Dictionary<int, int>>
            {
                Success = true,
                Data = counts,
                Message = "Donations per campaign count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, int>>> GetDonationsPerUserCountAsync()
        {
            var counts = await _repo.GetDonationsPerUserCountAsync();

            return new ServiceResponse<Dictionary<string, int>>
            {
                Success = true,
                Data = counts,
                Message = "Donations per user count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetTodayDonationsTotalAsync()
        {
            var total = await _repo.GetTodayDonationsTotalAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = "Today's donations total retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetThisWeekDonationsTotalAsync()
        {
            var total = await _repo.GetThisWeekDonationsTotalAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = "This week's donations total retrieved successfully."
            };
        }

        public async Task<ServiceResponse<double>> GetThisMonthDonationsTotalAsync()
        {
            var total = await _repo.GetThisMonthDonationsTotalAsync();

            return new ServiceResponse<double>
            {
                Success = true,
                Data = total,
                Message = "This month's donations total retrieved successfully."
            };
        }

        // ===== Financial Reporting =====

        public async Task<ServiceResponse<Dictionary<string, double>>> GetMonthlyDonationsReportAsync(int year)
        {
            var report = await _repo.GetMonthlyDonationsReportAsync(year);

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = report,
                Message = $"Monthly donations report for {year} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, double>>> GetQuarterlyDonationsReportAsync(int year)
        {
            var report = await _repo.GetQuarterlyDonationsReportAsync(year);

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = report,
                Message = $"Quarterly donations report for {year} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, double>>> GetYearlyDonationsReportAsync(int yearsBack = 5)
        {
            var report = await _repo.GetYearlyDonationsReportAsync(yearsBack);

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = report,
                Message = $"Yearly donations report for the last {yearsBack} years retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByTimeOfDayAsync()
        {
            var report = await _repo.GetDonationsByTimeOfDayAsync();

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = report,
                Message = "Donations by time of day retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, double>>> GetDonationsByDayOfWeekAsync()
        {
            var report = await _repo.GetDonationsByDayOfWeekAsync();

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = report,
                Message = "Donations by day of week retrieved successfully."
            };
        }

        // ===== Campaign Performance =====

        public async Task<ServiceResponse<Dictionary<DateTime, double>>> GetCampaignDonationTimelineAsync(int campaignId)
        {
            var timeline = await _repo.GetCampaignDonationTimelineAsync(campaignId);

            return new ServiceResponse<Dictionary<DateTime, double>>
            {
                Success = true,
                Data = timeline,
                Message = $"Donation timeline for campaign {campaignId} retrieved successfully."
            };
        }

        // ===== User Engagement =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetRecurringDonorsAsync(PaginationParametersDto parametersDto, int minDonations = 3)
        {
            var donations = await _repo.GetRecurringDonorsAsync(parametersDto.PageNumber, parametersDto.PageSize, minDonations);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Recurring donors with at least {minDonations} donations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetFirstTimeDonorsAsync(PaginationParametersDto parametersDto, DateTime startDate, DateTime endDate)
        {
            var donations = await _repo.GetFirstTimeDonorsAsync(parametersDto.PageNumber, parametersDto.PageSize, startDate, endDate);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"First-time donors from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<Dictionary<string, double>>> GetUserLifetimeValueAsync()
        {
            var lifetimeValues = await _repo.GetUserLifetimeValueAsync();

            return new ServiceResponse<Dictionary<string, double>>
            {
                Success = true,
                Data = lifetimeValues,
                Message = "User lifetime values retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<string>>> GetLoyalDonorsAsync(PaginationParametersDto parametersDto, double minTotalAmount = 1000, int minDonations = 5)
        {
            var (loyalDonors, totalCount) = await _repo.GetLoyalDonorsAsync(
                parametersDto.PageNumber,
                parametersDto.PageSize,
                minTotalAmount,
                minDonations);

            var response = new PagedResultDto<string>
            {
                Items = loyalDonors,
                TotalCount = totalCount,
                PageNumber = parametersDto.PageNumber,
                PageSize = parametersDto.PageSize
            };

            return new ServiceResponse<PagedResultDto<string>>
            {
                Success = true,
                Data = response,
                Message = $"Loyal donors with at least ${minTotalAmount} and {minDonations} donations retrieved successfully."
            };
        }

        // ===== Search & Filter Combinations =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> SearchDonationsByUserAndCampaignAsync(PaginationParametersDto parametersDto, string userId, int campaignId)
        {
            var donations = await _repo.SearchDonationsByUserAndCampaignAsync(parametersDto.PageNumber, parametersDto.PageSize, userId, campaignId);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donations for user {userId} and campaign {campaignId} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByMultipleUsersAsync(PaginationParametersDto parametersDto, IEnumerable<string> userIds)
        {
            var donations = await _repo.GetDonationsByMultipleUsersAsync(parametersDto.PageNumber, parametersDto.PageSize, userIds);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Donations by multiple users retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByMultipleCampaignsAsync(PaginationParametersDto parametersDto, IEnumerable<int> campaignIds)
        {
            var donations = await _repo.GetDonationsByMultipleCampaignsAsync(parametersDto.PageNumber, parametersDto.PageSize, campaignIds);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = "Donations by multiple campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetDonationsByAmountAndDateAsync(PaginationParametersDto parametersDto, double minAmount, DateTime startDate)
        {
            var donations = await _repo.GetDonationsByAmountAndDateAsync(parametersDto.PageNumber, parametersDto.PageSize, minAmount, startDate);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Donations with amount >= {minAmount} from {startDate:yyyy-MM-dd} retrieved successfully."
            };
        }

        // ===== Audit =====

        public async Task<ServiceResponse<PagedResultDto<DonationResponseDto>>> GetSuspiciousDonationsAsync(PaginationParametersDto parametersDto, double amountThreshold = 10000)
        {
            var donations = await _repo.GetSuspiciousDonationsAsync(parametersDto.PageNumber, parametersDto.PageSize, amountThreshold);
            var donationDtos = _mapper.MapToDonationResponseDtos(donations.Data);

            var response = donations.ToPagedResult(donationDtos, parametersDto);

            return new ServiceResponse<PagedResultDto<DonationResponseDto>>
            {
                Success = true,
                Data = response,
                Message = $"Suspicious donations exceeding ${amountThreshold} retrieved successfully."
            };
        }

        // ===== Export =====

        public async Task<ServiceResponse<int>> GetDonationRecordCountForPeriodAsync(DateTime startDate, DateTime endDate)
        {
            var count = await _repo.GetDonationRecordCountForPeriodAsync(startDate, endDate);

            return new ServiceResponse<int>
            {
                Success = true,
                Data = count,
                Message = $"Donation record count from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} retrieved successfully."
            };
        }
    }
}