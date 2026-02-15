namespace AccountService.Dtos;

/// <summary>
/// Модель счёта для ответа API.
/// </summary>
public class AccountResponse
{
    /// <summary>
    /// Уникальный идентификатор счёта.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор владельца счёта.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Тип счёта (Checking, Deposit, Credit).
    /// </summary>
    public Models.AccountType Type { get; set; }

    /// <summary>
    /// Код валюты (ISO 4217, например RUB, USD).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Текущий баланс.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Процентная ставка (для Deposit и Credit).
    /// </summary>
    public decimal? InterestRate { get; set; }

    /// <summary>
    /// Дата открытия счёта.
    /// </summary>
    public DateTime OpenDate { get; set; }

    /// <summary>
    /// Дата закрытия счёта (если применимо).
    /// </summary>
    public DateTime? CloseDate { get; set; }
}