using AccountService.Models;
using System.Collections.Concurrent;
using System.Security.Principal;

namespace AccountService.Repositories;

public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _accounts = new();
    private readonly Dictionary<Guid, Transaction> _transactions = new();
    private readonly object _lock = new();

    public InMemoryAccountRepository()
    {
        // Инициализация тестовыми данными (например, счета Ивана)
        var ownerId = Guid.NewGuid(); // допустим, это Иван
        var checking = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Type = AccountType.Checking,
            Currency = "RUB",
            Balance = 1000,
            InterestRate = null,
            OpenDate = DateTime.UtcNow.AddDays(-30),
            CloseDate = null,
            Transactions = new List<Transaction>()
        };
        var deposit = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Type = AccountType.Deposit,
            Currency = "RUB",
            Balance = 5000,
            InterestRate = 3.0m,
            OpenDate = DateTime.UtcNow.AddDays(-30),
            CloseDate = null,
            Transactions = new List<Transaction>()
        };
        _accounts[checking.Id] = checking;
        _accounts[deposit.Id] = deposit;
    }

    public Task<Account> CreateAccount(Account account)
    {
        lock (_lock) { _accounts[account.Id] = account; }
        return Task.FromResult(account);
    }

    public Task<Account?> GetAccountById(Guid id)
    {
        Account? account = null;
        lock (_lock)
        {
            _accounts.TryGetValue(id, out account);
        }
        return Task.FromResult(account);
    }

    public Task<IEnumerable<Account>> GetAccountsByOwner(Guid ownerId)
    {
        lock (_lock)
        {
            var accounts = _accounts.Values.Where(a => a.OwnerId == ownerId).ToList();
            return Task.FromResult(accounts.AsEnumerable());
        }
    }

    public Task<IEnumerable<Account>> GetAllAccounts()
    {
        lock (_lock) { return Task.FromResult(_accounts.Values.AsEnumerable()); }
    }

    public Task UpdateAccount(Account account)
    {
        lock (_lock) { _accounts[account.Id] = account; }
        return Task.CompletedTask;
    }

    public Task DeleteAccount(Guid id)
    {
        lock (_lock)
        {
            _accounts.Remove(id);
            var toRemove = _transactions.Values.Where(t => t.AccountId == id).Select(t => t.Id).ToList();
            foreach (var tid in toRemove) _transactions.Remove(tid);
        }
        return Task.CompletedTask;
    }

    public Task AddTransaction(Transaction transaction)
    {
        lock (_lock)
        {
            _transactions[transaction.Id] = transaction;
            if (_accounts.TryGetValue(transaction.AccountId, out var account))
                account.Transactions.Add(transaction);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Transaction>> GetTransactionsForAccount(Guid accountId, DateTime? from, DateTime? to)
    {
        lock (_lock)
        {
            var query = _transactions.Values.Where(t => t.AccountId == accountId);
            if (from.HasValue) query = query.Where(t => t.Timestamp >= from.Value);
            if (to.HasValue) query = query.Where(t => t.Timestamp <= to.Value);
            return Task.FromResult(query.OrderBy(t => t.Timestamp).AsEnumerable());
        }
    }

    public Task<bool> AccountExists(Guid id)
    {
        lock (_lock) { return Task.FromResult(_accounts.ContainsKey(id)); }
    }

    public Task<Transaction?> GetTransactionById(Guid id)
    {
        Transaction? transaction = null;
        lock (_lock)
        {
            _transactions.TryGetValue(id, out transaction);
        }
        return Task.FromResult(transaction);
    }
}