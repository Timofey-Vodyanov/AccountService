using AccountService.Dtos;

namespace AccountService.Services;

public interface IAccountService
{
    Task<IEnumerable<AccountResponse>> GetAccountsAsync(Guid? ownerId);
    Task<AccountResponse?> GetAccountByIdAsync(Guid id);
    Task<bool> AccountExistsAsync(Guid id);
    Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request);
    Task UpdateAccountAsync(Guid id, UpdateAccountRequest request);
    Task DeleteAccountAsync(Guid id);
    Task<IEnumerable<TransactionResponse>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to);
    Task<TransactionResponse> CreateTransactionAsync(Guid accountId, CreateTransactionRequest request);
}