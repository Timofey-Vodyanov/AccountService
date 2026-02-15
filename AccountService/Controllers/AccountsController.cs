using Microsoft.AspNetCore.Mvc;
using AccountService.Dtos;
using AccountService.Services;

namespace AccountService.Controllers;

[ApiController]
[Route("api/accounts")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    /// <summary>
    /// Возвращает список счетов, опционально отфильтрованных по владельцу.
    /// </summary>
    /// <param name="ownerId">Идентификатор владельца (опционально). Если не указан, возвращаются все счета.</param>
    /// <returns>Список счетов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountResponse>), 200)]
    public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAccounts([FromQuery] Guid? ownerId)
    {
        var result = await _accountService.GetAccountsAsync(ownerId);
        return Ok(result);
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
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
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
        var exists = await _accountService.AccountExistsAsync(id);
        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Создать новый счёт.
    /// </summary>
    /// <param name="request">Данные для создания счёта</param>
    /// <returns>Созданный счёт</returns>
    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<AccountResponse>> CreateAccount(CreateAccountRequest request)
    {
        try
        {
            var result = await _accountService.CreateAccountAsync(request);
            return CreatedAtAction(nameof(GetAccount), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Полностью обновить счёт.
    /// </summary>
    /// <param name="id">Идентификатор счёта</param>
    /// <param name="request">Новые данные счёта</param>
    [HttpPut("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateAccount(Guid id, UpdateAccountRequest request)
    {
        try
        {
            await _accountService.UpdateAccountAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
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
        try
        {
            await _accountService.DeleteAccountAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
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
        try
        {
            var result = await _accountService.GetAccountTransactionsAsync(accountId, from, to);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Зарегистрировать транзакцию по счёту (пополнение или списание).
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
        try
        {
            var result = await _accountService.CreateTransactionAsync(accountId, request);
            return CreatedAtAction(nameof(TransactionsController.GetTransaction), "Transactions", new { id = result.Id }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
