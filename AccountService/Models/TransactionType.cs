using System.Text.Json.Serialization;

namespace AccountService.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransactionType
{
    Credit,   // зачисление
    Debit     // списание
}