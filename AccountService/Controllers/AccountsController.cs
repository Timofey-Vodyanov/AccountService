using Microsoft.AspNetCore.Mvc;
using AccountService.Models;
using AccountService.Dtos;
using AccountService.Repositories;

namespace AccountService.Controllers;

[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IAccountRepository _repository;

    public AccountsController(IAccountRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Возвращает список счетов, опционально отфильтрованных по владельцу.
    /// </summary>
    /// <param name="ownerId">Идентификатор владельца (опционально)</param>
    /// <returns>Список счетов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountResponse>), 200)]
    public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAccounts([FromQuery] Guid? ownerId)
    {
        IEnumerable<Account> accounts;
        if (ownerId.HasValue)
            accounts = await _repository.GetAccountsByOwner(ownerId.Value);
        else
            accounts = await _repository.GetAllAccounts();

        return Ok(accounts.Select(MapToResponse));
    }

    /// <summary>
    /// Получить счёт по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор счёта</param>
    /// <returns>Счёт</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AccountResponse>> GetAccount(Guid id)
    {
        var account = await _repository.GetAccountById(id);
        if (account == null) return NotFound();
        return Ok(MapToResponse(account));
    }

    /// <summary>
    /// Проверить существование счёта.
    /// </summary>
    /// <param name="id">Идентификатор счёта</param>
    [HttpHead("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> HeadAccount(Guid id)
    {
        var exists = await _repository.AccountExists(id);
        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Создать новый счёт.
    /// </summary>
    /// <param name="request">Данные для создания</param>
    /// <returns>Созданный счёт</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AccountResponse>> CreateAccount(CreateAccountRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Валидация бизнес-правил
        if (request.Type != AccountType.Checking && request.InterestRate == null)
            return BadRequest("Для счетов Deposit и Credit необходимо указать процентную ставку.");
        if (request.Type == AccountType.Checking && request.InterestRate != null)
            return BadRequest("Для счёта Checking процентная ставка не допускается.");
        if (request.CloseDate.HasValue && request.CloseDate <= request.OpenDate)
            return BadRequest("Дата закрытия должна быть позже даты открытия.");
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            return BadRequest("Валюта должна быть трёхбуквенным кодом ISO 4217.");

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
        return CreatedAtAction(nameof(GetAccount), new { id = created.Id }, MapToResponse(created));
    }

    /// <summary>
    /// Полностью обновить счёт.
    /// </summary>
    /// <param name="id">Идентификатор счёта</param>
    /// <param name="request">Новые данные</param>
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAccount(Guid id, UpdateAccountRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existing = await _repository.GetAccountById(id);
        if (existing == null) return NotFound();

        // Валидация
        if (request.Type != AccountType.Checking && request.InterestRate == null)
            return BadRequest("Для счетов Deposit и Credit необходимо указать процентную ставку.");
        if (request.Type == AccountType.Checking && request.InterestRate != null)
            return BadRequest("Для счёта Checking процентная ставка не допускается.");
        if (request.CloseDate.HasValue && request.CloseDate <= request.OpenDate)
            return BadRequest("Дата закрытия должна быть позже даты открытия.");
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            return BadRequest("Валюта должна быть трёхбуквенным кодом ISO 4217.");

        existing.OwnerId = request.OwnerId;
        existing.Type = request.Type;
        existing.Currency = request.Currency.ToUpperInvariant();
        existing.InterestRate = request.InterestRate;
        existing.OpenDate = request.OpenDate;
        existing.CloseDate = request.CloseDate;

        await _repository.UpdateAccount(existing);
        return NoContent();
    }

    /// <summary>
    /// Удалить счёт.
    /// </summary>
    /// <param name="id">Идентификатор счёта</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteAccount(Guid id)
    {
        var existing = await _repository.GetAccountById(id);
        if (existing == null) return NotFound();
        await _repository.DeleteAccount(id);
        return NoContent();
    }

    /// <summary>
    /// Получить транзакции по счёту за период.
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <param name="from">Начало периода (опционально)</param>
    /// <param name="to">Конец периода (опционально)</param>
    /// <returns>Список транзакций</returns>
    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetAccountTransactions(
        Guid accountId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var account = await _repository.GetAccountById(accountId);
        if (account == null) return NotFound();

        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("Начало периода не может быть позже конца.");

        var transactions = await _repository.GetTransactionsForAccount(accountId, from, to);
        return Ok(transactions.Select(MapToTransactionResponse));
    }

    /// <summary>
    /// Зарегистрировать транзакцию по счёту.
    /// </summary>
    /// <param name="accountId">Идентификатор счёта</param>
    /// <param name="request">Данные транзакции</param>
    /// <returns>Созданная транзакция</returns>
    [HttpPost("{accountId}/transactions")]
    [ProducesResponseType(typeof(TransactionResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TransactionResponse>> CreateAccountTransaction(
        Guid accountId,
        CreateTransactionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (request.Amount <= 0)
            return BadRequest("Сумма должна быть положительной.");

        var account = await _repository.GetAccountById(accountId);
        if (account == null) return NotFound($"Счёт {accountId} не найден.");

        if (request.CounterpartyAccountId.HasValue)
        {
            var counterparty = await _repository.GetAccountById(request.CounterpartyAccountId.Value);
            if (counterparty == null)
                return BadRequest($"Контрагент счёт {request.CounterpartyAccountId} не найден.");
        }

        if (request.Type == TransactionType.Debit && account.Balance < request.Amount)
            return BadRequest("Недостаточно средств.");

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

        return CreatedAtAction(nameof(TransactionsController.GetTransaction), "Transactions", new { id = transaction.Id }, MapToTransactionResponse(transaction));
    }

    // Маппинги
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