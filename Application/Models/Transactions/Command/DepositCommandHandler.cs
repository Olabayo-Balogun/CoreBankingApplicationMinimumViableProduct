using Application.Interface.Persistence;
using Application.Model;
using Application.Models.Accounts.Response;
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
		private readonly AppSettings _appSettings;
		public DepositCommandHandler (IOptions<AppSettings> appsettings, IMapper mapper, ITransactionRepository transactionRepository, IAccountRepository accountRepository, IUserRepository userRepository)
		{
			_mapper = mapper;
			_transactionRepository = transactionRepository;
			_accountRepository = accountRepository;
			_userRepository = userRepository;
			_appSettings = appsettings.Value;
		}

		public async Task<RequestResponse<TransactionResponse>> Handle (DepositCommand request, CancellationToken cancellationToken)
		{
			if (!request.Currency.Equals ("Naira", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("Pound", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("Yuan", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("Dollar", StringComparison.OrdinalIgnoreCase) && !request.Currency.Equals ("Euro", StringComparison.OrdinalIgnoreCase))
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

			var payload = _mapper.Map<TransactionDto> (request);

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
