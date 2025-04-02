using System.Windows;
namespace Bank.Classes;

public class BankAccount
{ 
    public string AccountNumber { get; private set; }
    public DateTime OpenDate { get; private set; }
    public string FullName { get; private set; }
    public string PassportNumber { get; private set; }
    public DateTime DateBirth { get; private set; }
    public double Balance { get; private set; }
    public DateTime EndDate { get; private set; }
    public AccountStatus Status { get; private set; }

    public enum AccountStatus
    {
        Открыт,
        Закрыт
    }

    public BankAccount(string accountNumber,
        DateTime openDate,
        string fullName,
        string passportNumber,
        DateTime dateBirth,
        double balance,
        DateTime endDate)
    {
        AccountNumber = accountNumber;
        OpenDate = openDate;
        FullName = fullName;
        PassportNumber = passportNumber;
        DateBirth = dateBirth;
        Balance = balance;
        EndDate = endDate;
        Status = AccountStatus.Открыт;
    }

    public bool Deposit(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма пополнения должна быть положительной", nameof(amount));

        if (Status == AccountStatus.Закрыт)
            throw new InvalidOperationException("Нельзя пополнить закрытый счет");

        Balance += amount;
        return true;
    }

    public bool Withdraw(double amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Сумма снятия должна быть положительной", nameof(amount));

        if (Status == AccountStatus.Закрыт)
            throw new InvalidOperationException("Нельзя снять средства с закрытого счета");

        if (amount > Balance)
            return false;

        Balance -= amount;
        return true;
    }

    public bool CloseAccount()
    {
        if (Balance != 0)
            return false;

        Status = AccountStatus.Закрыт;
        return true;
    }

    public static string GenerateAccountNumber()
    {
        Random random = new Random();
        int firstDigit = random.Next(1, 10);
        string otherDigits = string.Concat(Enumerable.Range(0, 11).Select(_ => random.Next(0, 10)));
        return firstDigit.ToString() + otherDigits;
    }

    public string GetAccountInfo()
    {
        return $"ФИО: {FullName}{Environment.NewLine}" +
               $"Текущий баланс счета: {Balance}{Environment.NewLine}" +
               $"Паспорт: {PassportNumber}{Environment.NewLine}" +
               $"Дата рождения: {DateBirth:dd-MM-yyyy}{Environment.NewLine}" +
               $"Дата открытия счета: {OpenDate:dd-MM-yyyy}{Environment.NewLine}" +
               $"Номер счета: {AccountNumber}{Environment.NewLine}" +
               $"Статус счета: {Status}{Environment.NewLine}";
    }
}