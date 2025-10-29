using Application.Models.Accounts.Command;
using Application.Models.Accounts.Response;
using Application.Models.AuditLogs.Command;
using Application.Models.Banks.Command;
using Application.Models.Banks.Response;
using Application.Models.Branches.Command;
using Application.Models.Branches.Response;
using Application.Models.EmailLogs.Command;
using Application.Models.EmailLogs.Response;
using Application.Models.EmailRequests.Command;
using Application.Models.EmailRequests.Response;
using Application.Models.EmailTemplates.Command;
using Application.Models.EmailTemplates.Response;
using Application.Models.Transactions.Command;
using Application.Models.Transactions.Response;
using Application.Models.Uploads.Command;
using Application.Models.Uploads.Response;
using Application.Models.Users.Command;
using Application.Models.Users.Response;

using AutoMapper;

using Domain.DTO;
using Domain.Entities;

namespace Application.Profiles
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles ()
        {
            CreateMap<CreateAuditLogCommand, AuditLog> ().ReverseMap ();

            CreateMap<CreateAccountCommand, AccountDto> ().ReverseMap ();
            CreateMap<UpdateAccountCommand, AccountDto> ().ReverseMap ();
            CreateMap<AccountResponse, AccountDto> ().ReverseMap ();
            CreateMap<AccountDto, Account> ().ReverseMap ();
            CreateMap<Account, AccountResponse> ().ReverseMap ();

            CreateMap<CreateBankCommand, BankDto> ().ReverseMap ();
            CreateMap<UpdateBankCommand, BankDto> ().ReverseMap ();
            CreateMap<BankResponse, BankDto> ().ReverseMap ();
            CreateMap<BankDto, Bank> ().ReverseMap ();
            CreateMap<Bank, BankResponse> ().ReverseMap ();

            CreateMap<CreateBranchCommand, BranchDto> ().ReverseMap ();
            CreateMap<UpdateBranchCommand, BranchDto> ().ReverseMap ();
            CreateMap<BranchResponse, BranchDto> ().ReverseMap ();
            CreateMap<BranchDto, Branch> ().ReverseMap ();
            CreateMap<Branch, BranchResponse> ().ReverseMap ();

            CreateMap<EmailLogDto, EmailLog> ().ReverseMap ();
            CreateMap<CreateEmailLogCommand, EmailLogDto> ().ReverseMap ();
            CreateMap<EmailLogResponse, EmailLogDto> ().ReverseMap ();
            CreateMap<EmailLog, EmailLogResponse> ().ReverseMap ();

            CreateMap<EmailRequestDto, EmailRequest> ().ReverseMap ();
            CreateMap<CreateEmailCommand, EmailRequestDto> ().ReverseMap ();
            CreateMap<EmailRequestResponse, EmailRequestDto> ().ReverseMap ();
            CreateMap<EmailRequest, EmailRequestResponse> ().ReverseMap ();

            CreateMap<EmailTemplateDto, EmailTemplate> ().ReverseMap ();
            CreateMap<CreateEmailTemplateCommand, EmailTemplateDto> ().ReverseMap ();
            CreateMap<EmailTemplateResponse, EmailTemplateDto> ().ReverseMap ();
            CreateMap<EmailTemplate, EmailTemplateResponse> ().ReverseMap ();

            CreateMap<DepositCommand, TransactionDto> ().ReverseMap ();
            CreateMap<WithdrawCommand, TransactionDto> ().ReverseMap ();
            CreateMap<TransactionDto, Transaction> ().ReverseMap ();
            CreateMap<Transaction, TransactionResponse> ().ReverseMap ();

            CreateMap<UploadDto, Upload> ().ReverseMap ();
            CreateMap<CreateUploadCommand, UploadDto> ().ReverseMap ();
            CreateMap<UpdateUploadCommand, UploadDto> ().ReverseMap ();
            CreateMap<Upload, UploadResponse> ().ReverseMap ();

            CreateMap<UserDto, User> ().ReverseMap ();
            CreateMap<RegistrationCommand, UserDto> ().ReverseMap ();
            CreateMap<UserResponse, UserDto> ().ReverseMap ();
            CreateMap<User, UserResponse> ().ReverseMap ();
            CreateMap<User, LoginResponse> ().ReverseMap ();
        }
    }
}
