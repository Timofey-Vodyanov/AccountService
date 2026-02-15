using AccountService.Dtos;

namespace AccountService.Services;

public interface ITransactionService
{
    Task<TransactionResponse?> GetTransactionByIdAsync(Guid id);
    Task TransferAsync(TransferRequest request);
}