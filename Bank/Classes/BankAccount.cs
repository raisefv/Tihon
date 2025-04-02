using System.Windows;
namespace Bank.Classes;

public class BankAccount
{ 
    public string AccountNumber { get; private set; }
    public DateTime OpenDate { get; private set; }
    public string FullName { get; private set; }
    public string PassportNumber { get; private set; }
    public DateTime DateBirth { get; private set; }
    public double Balance { get; set; }
    public DateTime EndDate { get; private set; }
    public AccountStatus Status { get; private set; }

    public enum AccountStatus { Открыт, Закрыт }

    public BankAccount(string accountNumber, DateTime openDate, string fullName, string passportNumber, DateTime dateBirth, double balance, DateTime endDate)
    {
        AccountNumber = accountNumber;
        OpenDate = openDate;
        FullName = fullName;
        PassportNumber = passportNumber;
        DateBirth = dateBirth;
        EndDate = endDate;
        Status = AccountStatus.Открыт;
        Balance = balance;
    }

    public void Deposit(double amount)
    {
        if (amount <= 0)
        {
            MessageBox.Show("Сумма пополнения должна быть положительной.", "Ошибка", MessageBoxButton.OK);
            return;
        }

        Balance += amount;
    }

    public void Withdraw(double amount)
    {
        if (amount <= 0)
        {
            MessageBox.Show("Сумма снятия должна быть положительной.", "Ошибка", MessageBoxButton.OK);
            return;
        }

        if (amount > Balance)
        {
            MessageBox.Show("Недостаточно средств на карте.", "Ошибка", MessageBoxButton.OK);
            return;
        }

        Balance -= amount;
    }

    public static string GenerateAccountNumber()
    {
        Random random = new Random();
        int firstDigit = random.Next(1, 10);
        string otherDigits = string.Concat(Enumerable.Range(0, 11).Select(_ => random.Next(0, 10)));
        return firstDigit.ToString() + otherDigits;
    }

    public void CloseAccount()
    {
        if (Balance > 0)
            MessageBox.Show("Нельзя закрыть счет с ненулевым балансом.");

        Status = AccountStatus.Закрыт;
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

    public static BankAccount operator +(BankAccount account, double amount)
    {
        if (amount < 0)
            MessageBox.Show("Сумма пополнения должна быть положительной.");

        account.Balance += amount;
        return account;
    }

    public static BankAccount operator -(BankAccount account, double amount)
    {
        if (amount < 0)
            MessageBox.Show("Сумма списания должна быть положительной.");

        if (amount > account.Balance)
            MessageBox.Show("Недостаточно средств на счете.");

        account.Balance -= amount;
        return account;
    }
}