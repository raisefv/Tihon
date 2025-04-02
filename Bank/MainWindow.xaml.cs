using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bank.Classes;

namespace Bank;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<BankAccount> _BankAccount = new List<BankAccount>();

    private List<Transaction> _deposit = new List<Transaction>();
    private List<Transaction> _withdrawal = new List<Transaction>();
    private List<Transaction> _transfer = new List<Transaction>();

    public MainWindow()
    {
        InitializeComponent();
        BirthDatePicker.DisplayDateEnd = DateTime.Now.AddYears(-14);
        BirthDatePicker.DisplayDateStart = DateTime.Now.AddYears(-100);
    }

    private void OpenAccountButton(object sender, RoutedEventArgs e)
    {
        string fullName = FullNameTextBox.Text;
        string passport = PassportTextBox.Text;
        DateTime? birthDate = BirthDatePicker.SelectedDate;

        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(passport) || birthDate == null)
        {
            MessageBox.Show("Пожалуйста, заполните все поля", "Ошибка", MessageBoxButton.OK);
            return;
        }

        if (fullName.Any(char.IsDigit))
        {
            MessageBox.Show("Поле ФИО не должно содержать цифр", "Ошибка", MessageBoxButton.OK);
            FullNameTextBox.Clear();
            return;
        }
        
        Regex passportRegex = new Regex(@"^\d{3}-\d{3}-\d{4}$");
        if (!passportRegex.IsMatch(passport))
        {
            MessageBox.Show("Некорректный формат паспорта. Используйте формат: xxx-xxx-xxxx (где x - цифра)", 
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            PassportTextBox.Clear();
            PassportTextBox.Focus();
            return;
        }

        var newBankAccount = new BankAccount(BankAccount.GenerateAccountNumber(), DateTime.Now, fullName, passport, birthDate.Value, 0, DateTime.Now.AddYears(10));
        _BankAccount.Add(newBankAccount);

        UpdateUserComboBox();

        FullNameTextBox.Clear();
        PassportTextBox.Clear();
        BirthDatePicker.SelectedDate = null;

        MessageBox.Show("Счет успешно открыт!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CloseAccountButton(object sender, RoutedEventArgs e)
    {
        if (UserComboBox.SelectedItem == null)
        {
            MessageBox.Show("Выберите пользователя!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        string selectedUserName = UserComboBox.SelectedItem.ToString();
        var selectedAccount = _BankAccount.FirstOrDefault(acc => acc.FullName == selectedUserName);

        if (selectedAccount == null)
        {
            MessageBox.Show("Ошибка: Счет не найден!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        if (selectedAccount.Status == BankAccount.AccountStatus.Закрыт)
        {
            MessageBox.Show("Счет уже закрыт.", "Ошибка", MessageBoxButton.OK);
            return;
        }

        selectedAccount.CloseAccount();

        OutputUserTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                selectedAccount.GetAccountInfo();

        MessageBox.Show("Счет успешно закрыт.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DepositButton(object sender, RoutedEventArgs e)
    {
        if (UserComboBox.SelectedItem == null)
        {
            MessageBox.Show("Сначала выберите пользователя и карту!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        if (!double.TryParse(AmountTextBox.Text, out double amount) || amount <= 0)
        {
            MessageBox.Show("Введите корректную сумму!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        string selectedUserName = UserComboBox.SelectedItem.ToString();
        var selectedAccount = _BankAccount.FirstOrDefault(acc => acc.FullName == selectedUserName);

        if (selectedAccount == null)
        {
            MessageBox.Show("Ошибка при поиске счета!", "Ошибка", MessageBoxButton.OK);
            return;
        }
        
        selectedAccount.Deposit(amount);

        Transaction newTransaction = new Transaction(
            accountNumber: selectedAccount.AccountNumber,
            operation: Transaction.OperationType.Пополнение,
            timestamp: DateTime.Now,
            isSuccessful: true,
            amount: amount,
            getterAccountNumber: selectedAccount.AccountNumber,
            senderAccountName: selectedUserName,
            getterAccountName: selectedUserName);

        AddTransaction(newTransaction);

        OutputUserTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                 selectedAccount.GetAccountInfo();

        MessageBox.Show("Операция выполнена успешно!", "Успех", MessageBoxButton.OK);

        AmountTextBox.Clear();
    }

    private void WithdrawButton(object sender, RoutedEventArgs e)
    {
        if (UserComboBox.SelectedItem == null)
        {
            MessageBox.Show("Сначала выберите пользователя и карту!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        if (!double.TryParse(AmountTextBox.Text, out double amount) || amount <= 0)
        {
            MessageBox.Show("Введите корректную сумму!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        string selectedUserName = UserComboBox.SelectedItem.ToString();
        var selectedAccount = _BankAccount.FirstOrDefault(acc => acc.FullName == selectedUserName);

        if (selectedAccount == null)
        {
            MessageBox.Show("Ошибка при поиске счета!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        selectedAccount.Withdraw(amount);

        Transaction newTransaction = new Transaction(
            accountNumber: selectedAccount.AccountNumber,
            operation: Transaction.OperationType.Снятие,
            timestamp: DateTime.Now,
            isSuccessful: true,
            amount: amount,
            getterAccountNumber: "",
            senderAccountName: selectedUserName,
            getterAccountName: "");

        AddTransaction(newTransaction);

        OutputUserTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                 selectedAccount.GetAccountInfo();

        MessageBox.Show("Операция выполнена успешно!", "Успех", MessageBoxButton.OK);

        AmountTextBox.Clear();
    }

    private void TransferButton(object sender, RoutedEventArgs e)
    {
        if (UserComboBox.SelectedItem == null || UserComboBox2.SelectedItem == null)
        {
            MessageBox.Show("Выберите обоих пользователей!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        if (!double.TryParse(AmountTextBox.Text, out double amount) || amount <= 0)
        {
            MessageBox.Show("Введите корректную сумму!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        string senderUserName = UserComboBox.SelectedItem.ToString();
        string receiverUserName = UserComboBox2.SelectedItem.ToString();

        if (senderUserName == receiverUserName)
        {
            MessageBox.Show("Нельзя перевести средства самому себе!", "Ошибка", MessageBoxButton.OK);
            return;
        }

        var senderAccount = _BankAccount.FirstOrDefault(acc => acc.FullName == senderUserName);
        var receiverAccount = _BankAccount.FirstOrDefault(acc => acc.FullName == receiverUserName);

        if (senderAccount == null || receiverAccount == null)
        {
            MessageBox.Show("Оба пользователя должны иметь открытые счета!", "Ошибка", MessageBoxButton.OK);
            return;
        }
        
        senderAccount.Withdraw(amount);
        receiverAccount.Deposit(amount);

        Transaction newTransaction = new Transaction(
            accountNumber: senderAccount.AccountNumber,
            operation: Transaction.OperationType.Перевод,
            timestamp: DateTime.Now,
            isSuccessful: true,
            amount: amount,
            getterAccountNumber: receiverAccount.AccountNumber,
            senderAccountName: senderUserName,
            getterAccountName: receiverUserName
        );

        AddTransaction(newTransaction);

        OutputUserTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                 senderAccount.GetAccountInfo();
        OutputUserTextBox2.Text = "Выбранный пользователь:" + Environment.NewLine +
                                  receiverAccount.GetAccountInfo();

        MessageBox.Show("Перевод выполнен успешно!", "Успех", MessageBoxButton.OK);

        AmountTextBox.Clear();
    }

    private void TransactionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UserComboBox == null || UserComboBox.SelectedItem == null || TransactionTypeComboBox == null || TransactionTypeComboBox.SelectedItem == null)
        {
            return;
        }

        if (UserComboBox.SelectedItem is string selectedUserName)
        {
            var bankAccount = _BankAccount.FirstOrDefault(acc => acc.FullName == selectedUserName);
            if (bankAccount != null)
            {
                var Transaction = GetFilteredTransaction(bankAccount);
                UpdateTransactionOutput(bankAccount, OutputTransactionTextBox, Transaction);
            }
        }
    }

    private List<Transaction> GetFilteredTransaction(BankAccount account)
    {
        if (TransactionTypeComboBox == null || TransactionTypeComboBox.SelectedItem == null)
        {
            return new List<Transaction>();
        }

        var selectedType = (TransactionTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

        var allTransaction = _deposit.Concat(_withdrawal).Concat(_transfer).ToList();

        allTransaction = allTransaction.Where(t => t.AccountNumber == account.AccountNumber).ToList();

        if (string.IsNullOrEmpty(selectedType) || selectedType == "Все")
        {
            return allTransaction;
        }

        var filteredTransaction = allTransaction.Where(t =>
        {
            switch (selectedType)
            {
                case "Пополнение":
                    return t.Operation == Transaction.OperationType.Пополнение;
                case "Снятие":
                    return t.Operation == Transaction.OperationType.Снятие;
                case "Перевод":
                    return t.Operation == Transaction.OperationType.Перевод;
                default:
                    return false;
            }
        }).ToList();

        return filteredTransaction;
    }

    private void AddTransaction(Transaction transaction)
    {
        switch (transaction.Operation)
        {
            case Transaction.OperationType.Пополнение:
                _deposit.Add(transaction);
                break;
            case Transaction.OperationType.Снятие:
                _withdrawal.Add(transaction);
                break;
            case Transaction.OperationType.Перевод:
                _transfer.Add(transaction);
                break;
            default:
                throw new InvalidOperationException("Неизвестный тип транзакции.");
        }

        var bankAccount = _BankAccount.FirstOrDefault(acc => acc.AccountNumber == transaction.AccountNumber);
        if (bankAccount != null)
        {
            UpdateTransactionOutput(bankAccount, OutputTransactionTextBox, GetFilteredTransaction(bankAccount));
        }
    }

    private void UpdateTransactionOutput(BankAccount account, TextBox outputTransactionTextBox, List<Transaction> transactionList)
    {
        if (account == null || transactionList == null)
        {
            outputTransactionTextBox.Text = "Нет данных о транзакциях.";
            return;
        }

        if (transactionList.Count == 0)
        {
            outputTransactionTextBox.Text = "Нет совершенных транзакций.";
            return;
        }

        string output = "";

        foreach (var transaction in transactionList)
        {
            output += transaction.OutputTransaction() + Environment.NewLine;

            if (transaction.Operation == Transaction.OperationType.Перевод)
            {
                output += "Отправитель: " + transaction.SenderAccountName + Environment.NewLine;
                output += "Перевод на счет: " + transaction.GetterAccountNumber + Environment.NewLine;
                output += "Получатель: " + transaction.GetterAccountName + Environment.NewLine;
            }

            output += Environment.NewLine;
        }

        Dispatcher.Invoke(() =>
        {
            outputTransactionTextBox.Text = output;
        });
    }

    private void UserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedUser(UserComboBox, OutputUserTextBox);
        UpdateSelectedUser(UserComboBox2, OutputUserTextBox2);
    }

    private void UserComboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedUser(UserComboBox, OutputUserTextBox);
        UpdateSelectedUser(UserComboBox2, OutputUserTextBox2);
    }

    private void UpdateSelectedUser(ComboBox userComboBox, TextBox outputTextBox)
    {
        if (userComboBox?.SelectedItem is string selectedUserName)
        {
            var selectedUser = _BankAccount.FirstOrDefault(user => user.FullName == selectedUserName);

            if (selectedUser != null)
            {
                outputTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                      selectedUser.GetAccountInfo();
            }
            else
            {
                outputTextBox.Text = "Пользователь не найден или у него нет счета.";
            }
        }
    }

    private void UpdateUserComboBox()
    {
        UserComboBox.Items.Clear();
        UserComboBox2.Items.Clear();

        foreach (var user in _BankAccount)
        {
            UserComboBox.Items.Add(user.FullName);
            UserComboBox2.Items.Add(user.FullName);
        }

        if (_BankAccount.Any())
        {
            UserComboBox.SelectedIndex = 0;
            UserComboBox2.SelectedIndex = 0;
        }
    }
}