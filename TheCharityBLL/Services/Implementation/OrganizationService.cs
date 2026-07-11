using TheCharityBLL.DTOs;
using TheCharityBLL.DTOs.OrganizationContactMethodDTOs;
using TheCharityBLL.DTOs.OrganizationDTOs;
using TheCharityBLL.DTOs.PaymentInfoDTOs;
using TheCharityBLL.DTOs.UserDTOs;
using TheCharityBLL.Mapper;
using TheCharityBLL.Services.Abstraction;

using TheCharityDAL.Enums;
using TheCharityDAL.Repositories.Abstraction;

namespace TheCharityBLL.Services.Implementation
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;
        private readonly OrganizationMapper _mapper;
        public OrganizationService(IOrganizationRepository repository, IAuthorizationService authorizationService, IUserService userService)
        {
            _repository = repository;
            _mapper = new OrganizationMapper();
            _authorizationService = authorizationService;
            _userService = userService;
        }
        public async Task<ServiceResponse<OrgContactMethodResponseDto>> CreateContactMethod(CreateOrgContactMethodDto contactMethod)
        {
            if (!await _repository.OrganizationExistsAsync(contactMethod.CompanyId))
            {
                return new ServiceResponse<OrgContactMethodResponseDto>
                {
                    Success = false,
                    Message = $"Organization with ID {contactMethod.CompanyId} not found."
                };
            }
            if (await _repository.ContactMethodExistsAsync(contactMethod.CompanyId, contactMethod.Type, contactMethod.Value))
            {
                return new ServiceResponse<OrgContactMethodResponseDto>
                {
                    Success = false,
                    Message = "Contact method already exists for this organization.",
                };
            }
            var organizationContact = _mapper.MapToOrganizationContactMethod(contactMethod);
            var createdContactMethod = await _repository.AddContactMethodAsync(organizationContact);
            var contactMethodResponseDto = _mapper.MapToOrganizationContactMethodResponseDto(createdContactMethod);
            return new ServiceResponse<OrgContactMethodResponseDto>
            {
                Success = true,
                Message = "Contact method added successfully.",
                Data = contactMethodResponseDto
            };
        }

        public async Task<ServiceResponse<OrganizationResponseDto>> CreateOrganization(CreateOrganizationDto organizationDto)
        {
            if (await _repository.OrganizationNameExistsAsync(organizationDto.Name))
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = "Organization name already exists.",
                };
            }
            var organization = _mapper.MapToOrganization(organizationDto);
            var createdOrganization = await _repository.AddOrganizationAsync(organization);
            var organizationResponceDto = _mapper.MapToOrganizationResponseDto(createdOrganization);
            return new ServiceResponse<OrganizationResponseDto>
            {
                Success = true,
                Data = organizationResponceDto,
                Message = "Organization created successfully."
            };
        }

        //public async Task<ServiceResponse<PaymentInfoResponseDto>> CreatePaymentInfo(CreatePaymentInfoDto paymentInfo)
        //{
        //    if (!await _repository.OrganizationExistsAsync(paymentInfo.OrganizationId))
        //    {
        //        return new ServiceResponse<PaymentInfoResponseDto>
        //        {
        //            Success = false,
        //            Message = $"Organization with ID {paymentInfo.OrganizationId} not found."
        //        };
        //    }

        //    if (await _repository.HasPaymentInfoAsync(paymentInfo.OrganizationId))
        //    {
        //        return new ServiceResponse<PaymentInfoResponseDto>
        //        {
        //            Success = false,
        //            Message = "Organization already has payment info registered."
        //        };
        //    }
        //    var payment = _mapper.MapToPaymentInfo(paymentInfo);
        //    var createPayment = await _repository.AddPaymentInfoAsync(payment);
        //    var paymentDto = _mapper.MapToPaymentInfoResponseDto(createPayment);
        //    return new ServiceResponse<PaymentInfoResponseDto>
        //    {
        //        Success = true,
        //        Data = paymentDto,
        //        Message = "Payment info linked successfully."
        //    };
        //}

        public async Task<ServiceResponse<bool>> DeleteContactMethod(int contactMethodId)
        {
            if (await _repository.GetContactMethodByIdAsync(contactMethodId) == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Contact method with ID {contactMethodId} not found.",
                };
            }
            await _repository.DeleteContactMethodAsync(contactMethodId);
            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Contact method deleted successfully.",
            };
        }

        public async Task<ServiceResponse<bool>> DeleteOrganization(int id)
        {
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Organization with ID {id} not found.",
                };
            }
            await _repository.DeleteOrganizationAsync(id);
            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Organization deleted successfully."
            };
        }

        //public async Task<ServiceResponse<bool>> DeletePaymentInfo(int paymentInfoId)
        //{
        //    var paymentInfo = await _repository.GetPaymentInfoByIdAsync(paymentInfoId);
        //    if (paymentInfo == null)
        //    {
        //        return new ServiceResponse<bool>
        //        {
        //            Success = false,
        //            Message = $"Payment Info with ID {paymentInfoId} not found."
        //        };
        //    }

        //    await _repository.DeletePaymentInfoAsync(paymentInfoId);
        //    return new ServiceResponse<bool>
        //    {
        //        Success = true,
        //        Message = "Payment info deleted successfully."
        //    };
        //}

        public async Task<ServiceResponse<int>> GetActiveOrganizationsCount()
        {
            var activeCount = await _repository.GetActiveOrganizationsCountAsync();
            return new ServiceResponse<int>
            {
                Success = true,
                Data = activeCount,
                Message = "Active organizations count retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetAllOrganizations(bool includeDeleted = false)
        {
            var organizations = await _repository.GetAllOrganizationsAsync(includeDeleted);
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<OrgContactMethodResponseDto>> GetContactMethodById(int contactMethodId)
        {
            var contactMethod = await _repository.GetContactMethodByIdAsync(contactMethodId);
            if (contactMethod == null)
            {
                return new ServiceResponse<OrgContactMethodResponseDto>
                {
                    Success = false,
                    Message = $"Contact method with ID {contactMethodId} not found.",
                };
            }
            var contactMethodDto = _mapper.MapToOrganizationContactMethodResponseDto(contactMethod);
            return new ServiceResponse<OrgContactMethodResponseDto>
            {
                Success = true,
                Message = "Contact method retrieved successfully.",
                Data = contactMethodDto
            };
        }

        public async Task<ServiceResponse<int>> GetContactMethodCountByType(int organizationId, ContactType type)
        {
            if (!await _repository.OrganizationExistsAsync(organizationId))
            {
                return new ServiceResponse<int>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found.",
                };
            }
            var count = await _repository.GetContactMethodCountByTypeAsync(organizationId, type);
            return new ServiceResponse<int>
            {
                Success = true,
                Message = "Contact method count retrieved successfully.",
                Data = count
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>> GetContactMethodsByType(int organizationId, ContactType type)
        {
            if (!await _repository.OrganizationExistsAsync(organizationId))
            {
                return new ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found.",
                };
            }
            var contactMethods = await _repository.GetContactMethodsByTypeAsync(organizationId, type);
            if (!contactMethods.Any())
            {
                return new ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrgContactMethodResponseDto>(),
                    Message = "No contact methods found for this type."
                };
            }
            var contactMethodDtos = _mapper.MapToOrganizationContactMethodResponseDtos(contactMethods);
            return new ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>
            {
                Success = true,
                Message = "Contact methods retrieved successfully.",
                Data = contactMethodDtos
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetDeletedOrganizations()
        {
            var deletedOrganizations = await _repository.GetDeletedOrganizationsAsync();
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(deletedOrganizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Deleted organizations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<OrganizationResponseDto>> GetOrganizationById(int id)
        {
            var organization = await _repository.GetOrganizationByIdAsync(id);
            if (organization == null)
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = $"Organization with ID {id} not found.",
                };
            }
            var organizationDto = _mapper.MapToOrganizationResponseDto(organization);
            return new ServiceResponse<OrganizationResponseDto>
            {
                Success = true,
                Data = organizationDto,
                Message = "Organization retrieved successfully."
            };
        }

        public async Task<ServiceResponse<OrganizationResponseDto>> GetOrganizationByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = "Organization name cannot be empty."
                };
            }
            var organization = await _repository.GetOrganizationByNameAsync(name);
            if (organization == null)
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = $"Organization with name {name} not found.",
                };
            }
            var organizationDto = _mapper.MapToOrganizationResponseDto(organization);
            return new ServiceResponse<OrganizationResponseDto>
            {
                Success = true,
                Data = organizationDto,
                Message = "Organization retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>> GetOrganizationContactMethods(int organizationId)
        {
            if (!await _repository.OrganizationExistsAsync(organizationId))
            {
                return new ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>
                {
                    Success = false,
                    Message = $"Organization with ID {organizationId} not found.",
                };
            }
            var contactMethods = await _repository.GetOrganizationContactMethodsAsync(organizationId);
            if (!contactMethods.Any())
            {
                return new ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrgContactMethodResponseDto>(),
                    Message = "No contact methods found for this organization."
                };
            }
            var contactMethodDtos = _mapper.MapToOrganizationContactMethodResponseDtos(contactMethods);
            return new ServiceResponse<IEnumerable<OrgContactMethodResponseDto>>
            {
                Success = true,
                Message = "Contact methods retrieved successfully.",
                Data = contactMethodDtos
            };
        }

        public async Task<ServiceResponse<Dictionary<int, DateTime>>> GetOrganizationLastPaymentUpdate()
        {
            var result = await _repository.GetOrganizationLastPaymentUpdateAsync();

            return new ServiceResponse<Dictionary<int, DateTime>>
            {
                Success = true,
                Data = result,
                Message = "Organization last payment updates retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsByAddress(string address)
        {
            var organizations = await _repository.GetOrganizationsByAddressAsync(address);
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = $"No Organizations found in {address}."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsByCampaignCount(int minCampaigns = 1)
        {
            if (minCampaigns < 0)
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = false,
                    Message = "Please enter valid camings count"
                };
            }
            var organizations = await _repository.GetOrganizationsByCampaignCountAsync(minCampaigns);
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = $"Organizations with at least {minCampaigns} campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsByContactType(ContactType type)
        {
            var organizations = await _repository.GetOrganizationsByContactTypeAsync(type);
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = $"No organizations found with {type} contact method."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = $"Organizations with contact type {type} retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationDropDownListDto>>> GetOrganizationsDropDown()
        {
            var organizations = await _repository.GetOrganizationsDropDownAsync();
            var organizationDtos = _mapper.MapToOrganizationDropDownListDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationDropDownListDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations for dropdown list retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithActiveCampaigns()
        {
            var organizations = await _repository.GetOrganizationsWithActiveCampaignsAsync();
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = "No organizations found with active campaigns."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations with active campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithCompletedCampaigns()
        {
            var organizations = await _repository.GetOrganizationsWithCompletedCampaignsAsync();
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = "No organizations found with completed campaigns."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations with completed campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithoutCampaigns()
        {
            var organizations = await _repository.GetOrganizationsWithoutCampaignsAsync();
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = "All organizations already have campaigns."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations without campaigns retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithoutPaymentInfo()
        {
            var organizations = await _repository.GetOrganizationsWithoutPaymentInfoAsync();
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = "All organizations already have payment info."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations without payment info retrieved successfully."
            };
        }

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetOrganizationsWithValidPaymentInfo()
        {
            var organizations = await _repository.GetOrganizationsWithValidPaymentInfoAsync();
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = "No organizations found with valid payment info."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = "Organizations with valid payment info retrieved successfully."
            };
        }

        public async Task<ServiceResponse<OrganizationDetailsDto>> GetOrganizationDetails(int id)
        {
            var organization = await _repository.GetOrganizationWithDetailsAsync(id);
            if (organization == null)
            {
                return new ServiceResponse<OrganizationDetailsDto>
                {
                    Success = false,
                    Message = $"Organization with ID {id} not found.",
                };
            }
            var organizationDto = _mapper.MapToOrganizationDetailsDto(organization);
            return new ServiceResponse<OrganizationDetailsDto>
            {
                Success = true,
                Data = organizationDto,
                Message = "Organization retrieved successfully."
            };
        }

        public async Task<ServiceResponse<PaymentInfoResponseDto>> GetPaymentInfoById(int paymentInfoId)
        {
            var paymentInfo = await _repository.GetPaymentInfoByIdAsync(paymentInfoId);
            if (paymentInfo == null)
            {
                return new ServiceResponse<PaymentInfoResponseDto>
                {
                    Success = false,
                    Message = $"Payment Info with ID {paymentInfoId} not found."
                };
            }
            var paymentDto = _mapper.MapToPaymentInfoResponseDto(paymentInfo);
            return new ServiceResponse<PaymentInfoResponseDto>
            {
                Success = true,
                Data = paymentDto,
                Message = "Payment Info retrived successfully."
            };
        }

        //public async Task<ServiceResponse<PaymentInfoResponseDto>> GetPaymentInfoByOrganizationId(int organizationId)
        //{
        //    if (!await _repository.OrganizationExistsAsync(organizationId))
        //    {
        //        return new ServiceResponse<PaymentInfoResponseDto>
        //        {
        //            Success = false,
        //            Message = $"Organization with ID {organizationId} not found."
        //        };
        //    }
        //    var payment = await _repository.GetPaymentInfoByOrganizationIdAsync(organizationId);
        //    if (payment == null)
        //    {
        //        return new ServiceResponse<PaymentInfoResponseDto>
        //        {
        //            Success = false,
        //            Message = "No payment info found for this organization."
        //        };
        //    }
        //    var paymentDto = _mapper.MapToPaymentInfoResponseDto(payment);
        //    return new ServiceResponse<PaymentInfoResponseDto>
        //    {
        //        Success = true,
        //        Data = paymentDto,
        //        Message = $"Payment info for organization retrived successfully"
        //    };
        //}

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> GetRecentlyRegisteredOrganizations(int days = 7)
        {
            if (days < 0)
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = false,
                    Message = "Please enter valid day"
                };
            }
            var organizations = await _repository.GetRecentlyRegisteredOrganizationsAsync(days);
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = $"No organizations registered in the last {days} days."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = $"Organizations registered in the last {days} days retrieved successfully."
            };
        }

        public async Task<ServiceResponse<int>> GetTotalOrganizationsCount()
        {
            var totalCount = await _repository.GetTotalOrganizationsCountAsync();
            return new ServiceResponse<int>
            {
                Success = true,
                Data = totalCount,
                Message = "Total organizations count retrieved successfully."
            };

        }

        public async Task<ServiceResponse<bool>> RestoreContactMethod(int contactMethodId)
        {
            if (await _repository.GetContactMethodByIdAsync(contactMethodId) == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Contact method with ID {contactMethodId} not found.",
                };
            }
            await _repository.RestoreContactMethodAsync(contactMethodId);
            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Contact method restored successfully.",
            };
        }

        public async Task<ServiceResponse<bool>> RestoreOrganization(int id)
        {
            if (!await _repository.OrganizationExistsAsync(id))
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Organization with ID {id} not found.",
                };
            }
            await _repository.RestoreOrganizationAsync(id);
            return new ServiceResponse<bool>
            {
                Success = true,
                Message = "Organization restored successfully"
            };
        }

        //public async Task<ServiceResponse<bool>> RestorePaymentInfo(int paymentInfoId)
        //{
        //    if (await _repository.GetPaymentInfoByIdAsync(paymentInfoId) == null)
        //    {
        //        return new ServiceResponse<bool>
        //        {
        //            Success = false,
        //            Message = $"Payment Info with ID {paymentInfoId} not found."
        //        };
        //    }

        //    await _repository.RestorePaymentInfoAsync(paymentInfoId);
        //    return new ServiceResponse<bool>
        //    {
        //        Success = true,
        //        Message = "Payment info restored successfully."
        //    };
        //}

        public async Task<ServiceResponse<IEnumerable<OrganizationResponseDto>>> SearchOrganizations(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = false,
                    Message = "Search term cannot be empty."
                };
            }
            var organizations = await _repository.SearchOrganizationsAsync(searchTerm);
            if (!organizations.Any())
            {
                return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
                {
                    Success = true,
                    Data = Enumerable.Empty<OrganizationResponseDto>(),
                    Message = $"No organizations found matching search term '{searchTerm}'."
                };
            }
            var organizationDtos = _mapper.MapToOrganizationResponseDtos(organizations);
            return new ServiceResponse<IEnumerable<OrganizationResponseDto>>
            {
                Success = true,
                Data = organizationDtos,
                Message = $"Organizations matching search term '{searchTerm}' retrieved successfully."
            };
        }

        public async Task<ServiceResponse<OrgContactMethodResponseDto>> UpdateContactMethod(int id, UpdateOrgContactMethodDto contactMethod)
        {
            var existcontactMethod = await _repository.GetContactMethodByIdAsync(id);
            if (existcontactMethod == null)
            {
                return new ServiceResponse<OrgContactMethodResponseDto>
                {
                    Success = false,
                    Message = $"Contact method with ID {id} not found.",
                };
            }
            if (existcontactMethod.Value != contactMethod.Value || existcontactMethod.Type != contactMethod.Type)
            {
                if (await _repository.ContactMethodExistsAsync((int)existcontactMethod.CompanyId, (ContactType)contactMethod.Type, contactMethod.Value))
                {
                    return new ServiceResponse<OrgContactMethodResponseDto>
                    {
                        Success = false,
                        Message = "This contact method value already exists for this organization."
                    };

                }
            }

            existcontactMethod.EditValue(contactMethod.Value);
            existcontactMethod.EditType(contactMethod.Type);
            var update = await _repository.UpdateContactMethodAsync(existcontactMethod);
            var conatctDto = _mapper.MapToOrganizationContactMethodResponseDto(update);
            return new ServiceResponse<OrgContactMethodResponseDto>
            {
                Success = true,
                Data = conatctDto,
                Message = "Contact method updated successfully.",
            };
        }

        public async Task<ServiceResponse<OrganizationResponseDto>> UpdateOrganization(int id,UpdateOrganizationDto organization)
        {
            var existingOrganization = await _repository.GetOrganizationByIdAsync(id);
            if (existingOrganization == null)
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = $"Organization with ID {id} not found.",
                };
            }
            if (!string.IsNullOrWhiteSpace(organization.Name) && existingOrganization.Name != organization.Name)
            {
                if (await _repository.OrganizationNameExistsAsync(organization.Name))
                {
                    return new ServiceResponse<OrganizationResponseDto>
                    {
                        Success = false,
                        Message = "Organization name already exists.",
                    };
                }
                existingOrganization.EditName(organization.Name);
            }
            if (!string.IsNullOrWhiteSpace(organization.Address))
            {
                existingOrganization.EditAddress(organization.Address);
            }
            var updateOrganization = await _repository.UpdateOrganizationAsync(existingOrganization);
            var organizationDto = _mapper.MapToOrganizationResponseDto(updateOrganization);
            return new ServiceResponse<OrganizationResponseDto>
            {
                Success = true,
                Data = organizationDto,
                Message = "Organization updated successfully."
            };
        }

        //public async Task<ServiceResponse<PaymentInfoResponseDto>> UpdatePaymentInfo(int id,UpdatePaymentInfoDto paymentInfo)
        //{
        //    var existingPayment = await _repository.GetPaymentInfoByIdAsync(id);
        //    if (existingPayment == null)
        //    {
        //        return new ServiceResponse<PaymentInfoResponseDto>
        //        {
        //            Success = false,
        //            Message = $"Payment Info with ID {id} not found.",
        //        };
        //    }
        //    if (!string.IsNullOrEmpty(paymentInfo.ApiKey))
        //        existingPayment.EditApiKey(paymentInfo.ApiKey);

        //    if (!string.IsNullOrEmpty(paymentInfo.IntegrationId))
        //        existingPayment.EditIntegrationId(paymentInfo.IntegrationId);

        //    if (!string.IsNullOrEmpty(paymentInfo.IframeId))
        //        existingPayment.EditIframeId(paymentInfo.IframeId);

        //    if (!string.IsNullOrEmpty(paymentInfo.HmacKey))
        //        existingPayment.EditHmacKey(paymentInfo.HmacKey);

        //    var updatedPayment = await _repository.UpdatePaymentInfoAsync(existingPayment);

        //    var paymentDto = _mapper.MapToPaymentInfoResponseDto(updatedPayment);

        //    return new ServiceResponse<PaymentInfoResponseDto>
        //    {
        //        Success = true,
        //        Data = paymentDto,
        //        Message = "Payment Info updated successfully."
        //    };

        //}

        public async Task<ServiceResponse<OrganizationRoleResponseDto>> AddSubAdminAsync(int organizationId, string userId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<OrganizationRoleResponseDto>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                // Check if user exists
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new ServiceResponse<OrganizationRoleResponseDto>
                    {
                        Success = false,
                        Message = $"User with ID {userId} not found."
                    };
                }

                // Check if user is already an admin
                var isAdmin = await _authorizationService.IsOrganizationAdminAsync(userId, organizationId);
                if (isAdmin)
                {
                    return new ServiceResponse<OrganizationRoleResponseDto>
                    {
                        Success = false,
                        Message = "User is already an Organization Admin. Cannot assign as Sub-Admin."
                    };
                }

                // Add sub-admin role
                var role = await _repository.AddSubAdminAsync(organizationId, userId);

                var response = new OrganizationRoleResponseDto
                {
                    Id = role.Id,
                    OrganizationId = role.OrganizationId,
                    UserId = role.UserId,
                    Role = role.Role,
                    UserName = user.UserName,
                    UserEmail = user.Email,
                    UserFullName = user.FullName
                };

                return new ServiceResponse<OrganizationRoleResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Sub-admin added successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<OrganizationRoleResponseDto>
                {
                    Success = false,
                    Message = $"Error adding sub-admin: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> RemoveSubAdminAsync(int organizationId, string userId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                // Check if user is actually a sub-admin
                var isSubAdmin = await _repository.IsUserSubAdminAsync(organizationId, userId);
                if (!isSubAdmin)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "User is not a sub-admin of this organization."
                    };
                }

                // Remove sub-admin role
                await _repository.RemoveSubAdminAsync(organizationId, userId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = "Sub-admin removed successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Error removing sub-admin: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<IEnumerable<UserResponseDTO>>> GetOrganizationSubAdminsAsync(int organizationId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<IEnumerable<UserResponseDTO>>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                var users = await _repository.GetOrganizationSubAdminsAsync(organizationId);

                var response = users.Select(u => new UserResponseDTO
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FullName = u.FullName,
                    ImgPath = u.ImgPath,
                    IsDeleted = u.IsDeleted,
                    PhoneNumber = u.PhoneNumber,
                    Address = u.Address
                });

                return new ServiceResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = true,
                    Data = response,
                    Message = "Sub-admins retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<UserResponseDTO>>
                {
                    Success = false,
                    Message = $"Error retrieving sub-admins: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> IsUserSubAdminAsync(int organizationId, string userId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                var isSubAdmin = await _repository.IsUserSubAdminAsync(organizationId, userId);

                return new ServiceResponse<bool>
                {
                    Success = true,
                    Data = isSubAdmin,
                    Message = isSubAdmin ? "User is a sub-admin." : "User is not a sub-admin."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = $"Error checking sub-admin status: {ex.Message}"
                };
            }
        }

        // ===== NEW: Organization Admin Management =====

        public async Task<ServiceResponse<OrganizationResponseDto>> AssignOrganizationAdminAsync(int organizationId, string adminUserId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<OrganizationResponseDto>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                // Check if user exists
                var user = await _userService.GetUserByIdAsync(adminUserId);
                if (user == null)
                {
                    return new ServiceResponse<OrganizationResponseDto>
                    {
                        Success = false,
                        Message = $"User with ID {adminUserId} not found."
                    };
                }

                // Check if user is already a sub-admin and remove that role first
                var isSubAdmin = await _repository.IsUserSubAdminAsync(organizationId, adminUserId);
                if (isSubAdmin)
                {
                    await _repository.RemoveSubAdminAsync(organizationId, adminUserId);
                }

                // Assign as admin
                var updatedOrg = await _repository.AssignOrganizationAdminAsync(organizationId, adminUserId);
                var response = _mapper.MapToOrganizationResponseDto(updatedOrg);

                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Organization admin assigned successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = $"Error assigning organization admin: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<OrganizationResponseDto>> RemoveOrganizationAdminAsync(int organizationId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<OrganizationResponseDto>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                var updatedOrg = await _repository.RemoveOrganizationAdminAsync(organizationId);
                var response = _mapper.MapToOrganizationResponseDto(updatedOrg);

                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Organization admin removed successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = $"Error removing organization admin: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<OrganizationResponseDto>> TransferOrganizationAdminAsync(int organizationId, string newAdminUserId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<OrganizationResponseDto>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                // Check if user exists
                var user = await _userService.GetUserByIdAsync(newAdminUserId);
                if (user == null)
                {
                    return new ServiceResponse<OrganizationResponseDto>
                    {
                        Success = false,
                        Message = $"User with ID {newAdminUserId} not found."
                    };
                }

                // Check if user is already a sub-admin and remove that role first
                var isSubAdmin = await _repository.IsUserSubAdminAsync(organizationId, newAdminUserId);
                if (isSubAdmin)
                {
                    await _repository.RemoveSubAdminAsync(organizationId, newAdminUserId);
                }

                // Transfer admin
                var updatedOrg = await _repository.TransferOrganizationAdminAsync(organizationId, newAdminUserId);
                var response = _mapper.MapToOrganizationResponseDto(updatedOrg);

                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = true,
                    Data = response,
                    Message = "Organization admin transferred successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<OrganizationResponseDto>
                {
                    Success = false,
                    Message = $"Error transferring organization admin: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserResponseDTO?>> GetOrganizationAdminAsync(int organizationId)
        {
            try
            {
                // Check if organization exists
                var organization = await _repository.GetOrganizationByIdAsync(organizationId);
                if (organization == null)
                {
                    return new ServiceResponse<UserResponseDTO?>
                    {
                        Success = false,
                        Message = $"Organization with ID {organizationId} not found."
                    };
                }

                var admin = await _repository.GetOrganizationAdminAsync(organizationId);

                if (admin == null)
                {
                    return new ServiceResponse<UserResponseDTO?>
                    {
                        Success = true,
                        Data = null,
                        Message = "No admin assigned to this organization."
                    };
                }

                var response = new UserResponseDTO
                {
                    Id = admin.Id,
                    UserName = admin.UserName,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    ImgPath = admin.ImgPath,
                    IsDeleted = admin.IsDeleted,
                    PhoneNumber = admin.PhoneNumber,
                    Address = admin.Address
                };

                return new ServiceResponse<UserResponseDTO?>
                {
                    Success = true,
                    Data = response,
                    Message = "Organization admin retrieved successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserResponseDTO?>
                {
                    Success = false,
                    Message = $"Error retrieving organization admin: {ex.Message}"
                };
            }
        }
    }
}