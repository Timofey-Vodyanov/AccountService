using System.Text.Json.Serialization;

namespace AccountService.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AccountType
{
    Checking,
    Deposit,
    Credit
}