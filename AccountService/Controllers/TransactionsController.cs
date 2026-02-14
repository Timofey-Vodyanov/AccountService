using Microsoft.AspNetCore.Mvc;
using AccountService.Models;
using AccountService.Dtos;
using AccountService.Repositories;

namespace AccountService.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IAccountRepository _repository;

    public TransactionsController(IAccountRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Получить транзакцию по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор транзакции</param>
    /// <returns>Транзакция</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TransactionResponse>> GetTransaction(Guid id)
    {
        var transaction = await _repository.GetTransactionById(id);
        if (transaction == null) return NotFound();
        return Ok(MapToResponse(transaction));
    }

    /// <summary>
    /// Выполнить перевод между счетами.
    /// </summary>
    /// <param name="request">Данные перевода</param>
    [HttpPost("transfer")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.Amount <= 0)
            return BadRequest("Сумма должна быть положительной.");

        if (request.FromAccountId == request.ToAccountId)
            return BadRequest("Нельзя переводить на тот же счёт.");

        var fromAccount = await _repository.GetAccountById(request.FromAccountId);
        var toAccount = await _repository.GetAccountById(request.ToAccountId);
        if (fromAccount == null || toAccount == null)
            return BadRequest("Один или оба счёта не найдены.");

        if (fromAccount.Currency != request.Currency || toAccount.Currency != request.Currency)
            return BadRequest("Валюта перевода должна совпадать с валютой обоих счетов.");

        if (fromAccount.Balance < request.Amount)
            return BadRequest("Недостаточно средств на счёте отправителя.");

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

        return Ok(new { Message = "Перевод выполнен успешно." });
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