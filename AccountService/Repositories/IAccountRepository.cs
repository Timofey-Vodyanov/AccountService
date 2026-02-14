using AccountService.Models;
namespace AccountService.Repositories;

public interface IAccountRepository
{
    Task<Account> CreateAccount(Account account);
    Task<Account?> GetAccountById(Guid id);
    Task<IEnumerable<Account>> GetAccountsByOwner(Guid ownerId);
    Task<IEnumerable<Account>> GetAllAccounts();
    Task UpdateAccount(Account account);
    Task DeleteAccount(Guid id);
    Task AddTransaction(Transaction transaction);
    Task<IEnumerable<Transaction>> GetTransactionsForAccount(Guid accountId, DateTime? from, DateTime? to);
    Task<bool> AccountExists(Guid id);
    Task<Transaction?> GetTransactionById(Guid id);
}