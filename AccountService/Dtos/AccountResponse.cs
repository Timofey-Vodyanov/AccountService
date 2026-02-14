namespace AccountService.Dtos;

public class AccountResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Models.AccountType Type { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal? InterestRate { get; set; }
    public DateTime OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
}