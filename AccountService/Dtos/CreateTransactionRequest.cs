namespace AccountService.Dtos;

public class CreateTransactionRequest
{
    public Guid? CounterpartyAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Models.TransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
}