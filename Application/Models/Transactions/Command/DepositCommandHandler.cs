using Application.Interface.Persistence;
using Application.Models.Accounts.Response;
using Application.Models.IndustryField.Response;
using Application.Models.Transactions.Response;
using Application.Models.Users.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Enums;

using MediatR;

using Microsoft.Extensions.Options;

namespace Application.Models.Transactions.Command
{
    public class DepositCommandHandler : IRequestHandler<DepositCommand, RequestResponse<TransactionResponse>>
    {
        private readonly IMapper _mapper;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IUserRepository _userRepository;
        private readonly IIndustryFieldRepository _industryFieldRepository;
        private readonly AppSettings _appSettings;
        public DepositCommandHandler (IOptions<AppSettings> appsettings, IMapper mapper, ITransactionRepository transactionRepository, IAccountRepository accountRepository, IUserRepository userRepository, IIndustryFieldRepository industryFieldRepository)
        {
            _mapper = mapper;
            _transactionRepository = transactionRepository;
            _accountRepository = accountRepository;
            _userRepository = userRepository;
            _appSettings = appsettings.Value;
            _industryFieldRepository = industryFieldRepository;
        }

        public async Task<RequestResponse<TransactionResponse>> Handle (DepositCommand request, CancellationToken cancellationToken)
        {
            if (!request.Currency.Equals ("NGN", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("GBP", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("Yuan", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("USD", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("Euro", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResponse<TransactionResponse>.Failed (null, 400, "You can only deposit Naira, Dollar, Pound, Euro, or Yuan at this bank");
            }

            if (request.CreatedBy == null)
            {
                return RequestResponse<TransactionResponse>.NullPayload (null);
            }

            RequestResponse<AccountResponse> accountDetails = await _accountRepository.GetAccountByAccountNumberAsync (request.RecipientAccountNumber, request.CancellationToken);
            if (!accountDetails.IsSuccessful)
            {
                return RequestResponse<TransactionResponse>.NotFound (null, "Recipient account");
            }

            if (accountDetails.Data == null)
            {
                return RequestResponse<TransactionResponse>.NotFound (null, "Recipient account");
            }

            RequestResponse<UserResponse> userDetails = await _userRepository.GetUserFullNameByIdAsync (request.CreatedBy, request.CancellationToken);
            if (!userDetails.IsSuccessful)
            {
                return RequestResponse<TransactionResponse>.NotFound (null, "User");
            }

            if (userDetails.Data == null)
            {
                return RequestResponse<TransactionResponse>.NotFound (null, "User");
            }

            if (accountDetails.Data.AccountType != AccountType.NairaCurrent && accountDetails.Data.AccountType != AccountType.NairaSaving && request.Currency.Equals ("NGN", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResponse<TransactionResponse>.Failed (null, 400, "You can only deposit Naira into this account");
            }
            else if (accountDetails.Data.AccountType != AccountType.DollarCurrent && accountDetails.Data.AccountType != AccountType.DollarSaving && request.Currency.Equals ("USD", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResponse<TransactionResponse>.Failed (null, 400, "You can only deposit Dollar into this account");
            }
            else if (accountDetails.Data.AccountType != AccountType.PoundCurrent && accountDetails.Data.AccountType != AccountType.PoundSaving && request.Currency.Equals ("GBP", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResponse<TransactionResponse>.Failed (null, 400, "You can only deposit Pound into this account");
            }
            else if (accountDetails.Data.AccountType != AccountType.EuroSaving && accountDetails.Data.AccountType != AccountType.EuroCurrent && request.Currency.Equals ("Euro", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResponse<TransactionResponse>.Failed (null, 400, "You can only deposit Euro into this account");
            }
            else if (accountDetails.Data.AccountType != AccountType.YuanCurrent && accountDetails.Data.AccountType != AccountType.YuanSaving && request.Currency.Equals ("Yuan", StringComparison.OrdinalIgnoreCase))
            {
                return RequestResponse<TransactionResponse>.Failed (null, 400, "You can only deposit Yuan into this account");
            }

            RequestResponse<List<IndustryFieldResponse>> industryFields = await _industryFieldRepository.GetAllIndustryFieldsByIndustryIdAsync (userDetails.Data.IndustryId.GetValueOrDefault(), request.CancellationToken);
            if (!accountDetails.IsSuccessful)
            {
                return RequestResponse<TransactionResponse>.NotFound (null, "Industry field");
            }

            if(request.MetaData != null && industryFields.Data != null)
            {
                var validFields = industryFields.Data.ToDictionary (f => f.Name, f => f.DataType, StringComparer.OrdinalIgnoreCase);

                foreach (var field in industryFields.Data)
                {
                    if (field.IsRequired)
                    {
                        if (!request.MetaData.ContainsKey (field.Name))
                        {
                            return RequestResponse<TransactionResponse>.Failed (null, 400, $"The industry field {field.Name} is required for this transaction");
                        }
                    }                    
                }

                // Validate each metadata entry
                foreach (var meta in request.MetaData)
                {
                    // Check if the key exists in industry fields
                    if (!validFields.TryGetValue (meta.Key, out var expectedDataType))
                    {
                        return RequestResponse<TransactionResponse>.Failed (null, 400, $"The metadata key '{meta.Key}' is not a valid industry field");
                    }

                    // Validate that the value can be converted to the expected data type
                    if (!Utility.Utility.TryConvertToDataType (meta.Value, expectedDataType, out var conversionError))
                    {
                        return RequestResponse<TransactionResponse>.Failed (null, 400, $"The value for '{meta.Key}' cannot be converted to {expectedDataType}: {conversionError}");
                    }
                }
            }

            var payload = _mapper.Map<TransactionDto> (request);

            var depositAmount = await _transactionRepository.GetTotalTransactionDepositByAccountNumberAsync (request.RecipientAccountNumber, request.CancellationToken);

            if (depositAmount.Data != null && depositAmount.Data.Amount > _appSettings.MaximumDailyDepositLimitAmount)
            {
                payload.IsFlagged = true;
            }

            payload.TransactionType = TransactionType.Credit;
            payload.RecipientBankName = _appSettings.BankName;
            payload.RecipientAccountName = userDetails.Data.BusinessName ?? $"{userDetails.Data.FirstName} {userDetails.Data.LastName}";
            payload.PaymentService = _appSettings.DefaultPaymentService;
            payload.Channel = _appSettings.DefaultPaymentChannel;

            var result = await _transactionRepository.CreateTransactionAsync (payload);

            return result;
        }
    }
}
