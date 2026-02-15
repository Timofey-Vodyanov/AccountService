using AccountService.Dtos;
using AccountService.Models;
using AccountService.Repositories;

namespace AccountService.Services;

public class TransactionService : ITransactionService
{
    private readonly IAccountRepository _repository;

    public TransactionService(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<TransactionResponse?> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _repository.GetTransactionById(id);
        return transaction == null ? null : MapToResponse(transaction);
    }

    public async Task TransferAsync(TransferRequest request)
    {
        if (request.Amount <= 0)
            throw new ArgumentException("Сумма должна быть положительной.");

        if (request.FromAccountId == request.ToAccountId)
            throw new ArgumentException("Нельзя переводить на тот же счёт.");

        var fromAccount = await _repository.GetAccountById(request.FromAccountId);
        var toAccount = await _repository.GetAccountById(request.ToAccountId);
        if (fromAccount == null || toAccount == null)
            throw new ArgumentException("Один или оба счёта не найдены.");

        if (fromAccount.Currency != request.Currency || toAccount.Currency != request.Currency)
            throw new ArgumentException("Валюта перевода должна совпадать с валютой обоих счетов.");

        if (fromAccount.Balance < request.Amount)
            throw new InvalidOperationException("Недостаточно средств на счёте отправителя.");

        var timestamp = DateTime.UtcNow;
        var debitTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = fromAccount.Id,
            CounterpartyAccountId = toAccount.Id,
            Amount = request.Amount,
            Currency = request.Currency,
            Type = TransactionType.Debit,
            Description = request.Description ?? "Перевод",
            Timestamp = timestamp
        };
        var creditTransaction = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = toAccount.Id,
            CounterpartyAccountId = fromAccount.Id,
            Amount = request.Amount,
            Currency = request.Currency,
            Type = TransactionType.Credit,
            Description = request.Description ?? "Перевод",
            Timestamp = timestamp
        };

        fromAccount.Balance -= request.Amount;
        toAccount.Balance += request.Amount;

        await _repository.AddTransaction(debitTransaction);
        await _repository.AddTransaction(creditTransaction);
        await _repository.UpdateAccount(fromAccount);
        await _repository.UpdateAccount(toAccount);
    }

    private TransactionResponse MapToResponse(Transaction t) => new()
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