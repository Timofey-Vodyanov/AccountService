namespace AccountService.Dtos;

/// <summary>
/// Запрос на создание транзакции по счёту.
/// </summary>
public class CreateTransactionRequest
{
    /// <summary>
    /// Идентификатор счёта контрагента (для переводов). Необязательное поле.
    /// </summary>
    public Guid? CounterpartyAccountId { get; set; }

    /// <summary>
    /// Сумма транзакции. Должна быть положительной.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Код валюты (ISO 4217, например RUB, USD).
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
}