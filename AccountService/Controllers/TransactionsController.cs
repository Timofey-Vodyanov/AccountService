using Microsoft.AspNetCore.Mvc;
using AccountService.Dtos;
using AccountService.Services;

namespace AccountService.Controllers;

[ApiController]
[Route("api/transactions")]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TransactionResponse>> GetTransaction(Guid id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        if (transaction == null) return NotFound();
        return Ok(transaction);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        try
        {
            await _transactionService.TransferAsync(request);
            return Ok(new { Message = "Перевод выполнен успешно." });
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