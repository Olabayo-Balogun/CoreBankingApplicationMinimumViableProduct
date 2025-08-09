using Application.Model;
using Application.Model.Payments.Command;
using Application.Model.Payments.Queries;
using Application.Models.Payments.Command;
using Application.Models.Payments.Queries;
using Application.Models.Payments.Response;

using Domain.DTO;

namespace Application.Interface.Persistence
{
	public interface IPaymentRepository
	{
		Task<RequestResponse<PaymentResponse>> CreatePaymentAsync (PaymentDto addPaymentViewModel);
		Task<RequestResponse<PaymentResponse>> VerifyPaymentAsync (VerifyPaymentCommand verifyPaymentViewModel);
		Task<RequestResponse<List<PaymentResponse>>> GetPaymentsByDatePaidAsync (GetPaymentByDatePaidQuery getPaymentByDatePaidViewModel);
		Task<RequestResponse<List<PaymentResponse>>> GetPaymentByAmountPaidAsync (GetPaymentByAmountPaidQuery getPaymentByAmountPaidViewModel);
		Task<RequestResponse<List<PaymentResponse>>> GetAllPaymentsAsync (GetAllPaymentQuery getAllPaymentViewModel);
		Task<RequestResponse<PaymentResponse>> GetPaymentByIdAsync (GetPaymentByIdQuery getDeletedPaymentViewModel);
		Task<RequestResponse<List<PaymentResponse>>> GetPaymentsByCustomerIdAsync (GetPaymentByCustomerIdQuery getPaymentByCustomerIdViewModel);
		Task<RequestResponse<List<PaymentResponse>>> GetPaymentByChannelAsync (GetPaymentByChannelQuery getPaymentByChannelViewModel);
		Task<RequestResponse<PaymentResponse>> DeletePaymentAsync (DeletePaymentCommand deleteBankViewModel);
		Task<RequestResponse<PaymentResponse>> ConfirmPaymentAsync (CancellationToken cancellationToken, decimal amount, string paymentReferenceNumber);
		Task<RequestResponse<List<PaymentResponse>>> GetPaymentByConfirmationStatusAsync (string userId, bool isConfirmed, CancellationToken cancellationToken, int pageNumber, int pageSize);
	}
}
