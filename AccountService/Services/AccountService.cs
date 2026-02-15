using AccountService.Dtos;
using AccountService.Models;
using AccountService.Repositories;

namespace AccountService.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _repository;

    public AccountService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<AccountResponse>> GetAccountsAsync(Guid? ownerId)
    {
        IEnumerable<Account> accounts;
        if (ownerId.HasValue)
            accounts = await _repository.GetAccountsByOwner(ownerId.Value);
        else
            accounts = await _repository.GetAllAccounts();

        return accounts.Select(MapToResponse);
    }

    public async Task<AccountResponse?> GetAccountByIdAsync(Guid id)
    {
        var account = await _repository.GetAccountById(id);
        return account == null ? null : MapToResponse(account);
    }

    public async Task<bool> AccountExistsAsync(Guid id)
    {
        return await _repository.AccountExists(id);
    }

    public async Task<AccountResponse> CreateAccountAsync(CreateAccountRequest request)
    {
        if (request.Type != AccountType.Checking && request.InterestRate == null)
            throw new ArgumentException("Для счетов Deposit и Credit необходимо указать процентную ставку.");
        if (request.Type == AccountType.Checking && request.InterestRate != null)
            throw new ArgumentException("Для счёта Checking процентная ставка не допускается.");
        if (request.CloseDate.HasValue && request.CloseDate <= request.OpenDate)
            throw new ArgumentException("Дата закрытия должна быть позже даты открытия.");
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            throw new ArgumentException("Валюта должна быть трёхбуквенным кодом ISO 4217.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            OwnerId = request.OwnerId,
            Type = request.Type,
            Currency = request.Currency.ToUpperInvariant(),
            Balance = 0,
            InterestRate = request.InterestRate,
            OpenDate = request.OpenDate,
            CloseDate = request.CloseDate
        };

        var created = await _repository.CreateAccount(account);
        return MapToResponse(created);
    }

    public async Task UpdateAccountAsync(Guid id, UpdateAccountRequest request)
    {
        var existing = await _repository.GetAccountById(id);
        if (existing == null)
            throw new KeyNotFoundException($"Счёт с id {id} не найден.");

        
        if (request.Type != AccountType.Checking && request.InterestRate == null)
            throw new ArgumentException("Для счетов Deposit и Credit необходимо указать процентную ставку.");
        if (request.Type == AccountType.Checking && request.InterestRate != null)
            throw new ArgumentException("Для счёта Checking процентная ставка не допускается.");
        if (request.CloseDate.HasValue && request.CloseDate <= request.OpenDate)
            throw new ArgumentException("Дата закрытия должна быть позже даты открытия.");
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            throw new ArgumentException("Валюта должна быть трёхбуквенным кодом ISO 4217.");

        existing.OwnerId = request.OwnerId;
        existing.Type = request.Type;
        existing.Currency = request.Currency.ToUpperInvariant();
        existing.InterestRate = request.InterestRate;
        existing.OpenDate = request.OpenDate;
        existing.CloseDate = request.CloseDate;

        await _repository.UpdateAccount(existing);
    }

    public async Task DeleteAccountAsync(Guid id)
    {
        var existing = await _repository.GetAccountById(id);
        if (existing == null)
            throw new KeyNotFoundException($"Счёт с id {id} не найден.");

        await _repository.DeleteAccount(id);
    }

    public async Task<IEnumerable<TransactionResponse>> GetAccountTransactionsAsync(Guid accountId, DateTime? from, DateTime? to)
    {
        var account = await _repository.GetAccountById(accountId);
        if (account == null)
            throw new KeyNotFoundException($"Счёт {accountId} не найден.");

        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("Начало периода не может быть позже конца.");

        var transactions = await _repository.GetTransactionsForAccount(accountId, from, to);
        return transactions.Select(MapToTransactionResponse);
    }

    public async Task<TransactionResponse> CreateTransactionAsync(Guid accountId, CreateTransactionRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Сумма должна быть положительной.");

        var account = await _repository.GetAccountById(accountId);
        if (account == null)
            throw new KeyNotFoundException($"Счёт {accountId} не найден.");

        if (request.CounterpartyAccountId.HasValue)
        {
            var counterparty = await _repository.GetAccountById(request.CounterpartyAccountId.Value);
            if (counterparty == null)
                throw new ArgumentException($"Контрагент счёт {request.CounterpartyAccountId} не найден.");
        }

        if (request.Type == TransactionType.Debit && account.Balance < request.Amount)
            throw new InvalidOperationException("Недостаточно средств.");

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            CounterpartyAccountId = request.CounterpartyAccountId,
            Amount = request.Amount,
            Currency = request.Currency,
            Type = request.Type,
            Description = request.Description,
            Timestamp = DateTime.UtcNow
        };

        if (request.Type == TransactionType.Credit)
            account.Balance += request.Amount;
        else
            account.Balance -= request.Amount;

        await _repository.AddTransaction(transaction);
        await _repository.UpdateAccount(account);

        return MapToTransactionResponse(transaction);
    }

    private AccountResponse MapToResponse(Account account) => new()
    {
        Id = account.Id,
        OwnerId = account.OwnerId,
        Type = account.Type,
        Currency = account.Currency,
        Balance = account.Balance,
        InterestRate = account.InterestRate,
        OpenDate = account.OpenDate,
        CloseDate = account.CloseDate
    };

    private TransactionResponse MapToTransactionResponse(Transaction t) => new()
    {
        Id = t.Id,
        AccountId = t.AccountId,
        CounterpartyAccountId = t.CounterpartyAccountId,
        Amount = t.Amount,
        Currency = t.Currency,
        Type = t.Type,
        Description = t.Description,
        Timestamp = t.Timestamp
    };
}