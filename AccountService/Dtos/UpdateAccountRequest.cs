namespace AccountService.Dtos;

/// <summary>
/// Запрос на полное обновление счёта.
/// </summary>
public class UpdateAccountRequest
{
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
    /// Процентная ставка (обязательна для Deposit и Credit, для Checking должна быть null).
    /// </summary>
    public decimal? InterestRate { get; set; }

    /// <summary>
    /// Дата открытия счёта.
    /// </summary>
    public DateTime OpenDate { get; set; }

    /// <summary>
    /// Дата закрытия счёта (опционально).
    /// </summary>
    public DateTime? CloseDate { get; set; }
}