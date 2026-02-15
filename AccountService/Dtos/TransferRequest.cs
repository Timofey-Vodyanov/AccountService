namespace AccountService.Dtos;

/// <summary>
/// Запрос на перевод средств между счетами.
/// </summary>
public class TransferRequest
{
    /// <summary>
    /// Идентификатор счёта-отправителя.
    /// </summary>
    public Guid FromAccountId { get; set; }

    /// <summary>
    /// Идентификатор счёта-получателя.
    /// </summary>
    public Guid ToAccountId { get; set; }

    /// <summary>
    /// Сумма перевода (должна быть положительной).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Код валюты (ISO 4217) (должна совпадать с валютой обоих счетов).
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    /// Описание перевода (необязательное).
    /// </summary>
    public string Description { get; set; } = string.Empty;
}