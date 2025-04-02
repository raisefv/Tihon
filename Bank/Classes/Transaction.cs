using System.Text;

namespace Bank.Classes;

public class Transaction
{
    public string AccountNumber { get; }
    public OperationType Operation { get; }
    public DateTime Timestamp { get; }
    public double Amount { get; }
    public string GetterAccountNumber { get; }
    public string GetterAccountName { get; }
    public string SenderAccountName { get; }

    public enum OperationType { Снятие, Пополнение, Перевод }

    public Transaction(string accountNumber,
        OperationType operation,
        DateTime timestamp,
        double amount,
        string getterAccountNumber = null,
        string senderAccountName = null,
        string getterAccountName = null)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Номер счета обязателен", nameof(accountNumber));

        if (amount <= 0)
            throw new ArgumentException("Сумма должна быть положительной", nameof(amount));

        AccountNumber = accountNumber;
        Operation = operation;
        Timestamp = timestamp;
        Amount = amount;
        GetterAccountNumber = getterAccountNumber;
        SenderAccountName = senderAccountName;
        GetterAccountName = getterAccountName;
    }

    public string OutputTransaction()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Операция: {Operation}");
        sb.AppendLine($"Счет: {AccountNumber}");
        sb.AppendLine($"Сумма: {Amount}");
        sb.AppendLine($"Дата: {Timestamp:dd.MM.yyyy HH:mm:ss}");

        if (Operation == OperationType.Перевод)
        {
            sb.AppendLine($"Получатель: {GetterAccountName}");
            sb.AppendLine($"Счет получателя: {GetterAccountNumber}");
        }

        return sb.ToString();
    }
}