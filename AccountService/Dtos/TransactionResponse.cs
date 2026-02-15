namespace AccountService.Dtos;

/// <summary>
/// Модель транзакции для ответа API.
/// </summary>
public class TransactionResponse
{
    /// <summary>
    /// Уникальный идентификатор транзакции.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Идентификатор счёта, к которому относится транзакция.
    /// </summary>
    public Guid AccountId { get; set; }

    /// <summary>
    /// Идентификатор счёта контрагента (для переводов).
    /// </summary>
    public Guid? CounterpartyAccountId { get; set; }

    /// <summary>
    /// Сумма транзакции.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Код валюты (ISO 4217).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Тип транзакции (Credit – зачисление, Debit – списание).
    /// </summary>
    public Models.TransactionType Type { get; set; }

    /// <summary>
    /// Описание транзакции.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время совершения транзакции.
    /// </summary>
    public DateTime Timestamp { get; set; }
}