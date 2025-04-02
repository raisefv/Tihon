namespace Bank.Classes;

public class Transaction
{
    public string AccountNumber { get; set; }
    public OperationType Operation { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSuccessful { get; set; }
    public double Amount { get; set; }
    public enum OperationType { Снятие, Пополнение, Перевод }
    public string GetterAccountNumber { get; set; }
    public string GetterAccountName { get; set; }
    public string SenderAccountName { get; set; }

    public Transaction(string accountNumber, OperationType operation, DateTime timestamp, bool isSuccessful, double amount, string getterAccountNumber, string senderAccountName, string getterAccountName)
    {
        AccountNumber = accountNumber;
        Operation = operation;
        Timestamp = timestamp;
        IsSuccessful = isSuccessful;
        Amount = amount;
        GetterAccountNumber = getterAccountNumber;
        GetterAccountName = getterAccountName;
        SenderAccountName = senderAccountName;
    }

    public string OutputTransaction()
    {
        string transactionInfo = $"Операция: {this.Operation}{Environment.NewLine}" +
                                 $"Счет: {this.AccountNumber}{Environment.NewLine}" +
                                 $"Сумма: {this.Amount}{Environment.NewLine}" +
                                 $"Результат транзакции: {(this.IsSuccessful ? "успешно" : "неудача")}{Environment.NewLine}" +
                                 $"Дата: {this.Timestamp.ToString("dd-MM-yyyy HH:mm:ss")}{Environment.NewLine}";

        return transactionInfo;
    }
}