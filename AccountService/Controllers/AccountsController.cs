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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AccountResponse>), 200)]
    public async Task<ActionResult<IEnumerable<AccountResponse>>> GetAccounts([FromQuery] Guid? ownerId)
    {
        var result = await _accountService.GetAccountsAsync(ownerId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AccountResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AccountResponse>> GetAccount(Guid id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpHead("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> HeadAccount(Guid id)
    {
        var exists = await _accountService.AccountExistsAsync(id);
        return exists ? Ok() : NotFound();
    }

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