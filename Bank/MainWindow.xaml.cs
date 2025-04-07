using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Bank.Classes;
using Newtonsoft.Json;

namespace Bank;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private List<BankAccount> _bankAccounts = new List<BankAccount>();
    private List<Transaction> _deposits = new List<Transaction>();
    private List<Transaction> _withdrawals = new List<Transaction>();
    private List<Transaction> _transfers = new List<Transaction>();

    public MainWindow()
    {
        InitializeComponent();
        ClientBirthDatePicker.DisplayDateEnd = DateTime.Now.AddYears(-14);
        ClientBirthDatePicker.DisplayDateStart = DateTime.Now.AddYears(-100);
    }

    private void OnOpenAccountButtonClick(object sender, RoutedEventArgs e)
    {
        string fullName = ClientFullNameTextBox.Text;
        string passport = ClientPassportTextBox.Text;
        DateTime? birthDate = ClientBirthDatePicker.SelectedDate;

        if (string.IsNullOrWhiteSpace(fullName) || 
            string.IsNullOrWhiteSpace(passport) || 
            birthDate == null)
        {
            ShowErrorMessage("Пожалуйста, заполните все поля");
            return;
        }

        if (fullName.Any(char.IsDigit))
        {
            ShowErrorMessage("Поле ФИО не должно содержать цифр");
            ClientFullNameTextBox.Clear();
            return;
        }
        
        var passportRegex = new Regex(@"^\d{6}-\d{4}$");
        if (!passportRegex.IsMatch(passport))
        {
            ShowErrorMessage("Некорректный формат паспорта. Используйте формат: xxxxxx-xxxx");
            ClientPassportTextBox.Clear();
            ClientPassportTextBox.Focus();
            return;
        }

        var newBankAccount = new BankAccount(
            BankAccount.GenerateAccountNumber(), 
            DateTime.Now, 
            fullName, 
            passport, 
            birthDate.Value, 
            0, 
            DateTime.Now.AddYears(10));
        
        _bankAccounts.Add(newBankAccount);
        UpdateClientSelectors();

        ClientFullNameTextBox.Clear();
        ClientPassportTextBox.Clear();
        ClientBirthDatePicker.SelectedDate = null;
        
        ShowSuccessMessage("Счет успешно открыт!");
    }

    private void OnCloseAccountButtonClick(object sender, RoutedEventArgs e)
    {
        if (FirstClientSelector.SelectedItem == null)
        {
            ShowErrorMessage("Выберите пользователя!");
            return;
        }
        
        string selectedUserName = FirstClientSelector.SelectedItem.ToString();
        var selectedAccount = _bankAccounts.FirstOrDefault(acc => acc.FullName == selectedUserName);

        if (selectedAccount == null)
        {
            ShowErrorMessage("Ошибка: Счет не найден!");
            return;
        }

        if (selectedAccount.Status == BankAccount.AccountStatus.Закрыт)
        {
            ShowErrorMessage("Счет уже закрыт.");
            return;
        }

        selectedAccount.CloseAccount();

        FirstClientInfoTextBox.Text = $"Выбранный пользователь:{Environment.NewLine}{selectedAccount.GetAccountInfo()}";
        ShowSuccessMessage("Счет успешно закрыт.");
    }

    private void OnDepositButtonClick(object sender, RoutedEventArgs e)
    {
        if (FirstClientSelector.SelectedItem == null)
        {
            ShowErrorMessage("Сначала выберите пользователя!");
            return;
        }

        string selectedUserName = FirstClientSelector.SelectedItem.ToString();
        var selectedAccount = _bankAccounts.FirstOrDefault(acc => acc.FullName == selectedUserName);

        if (selectedAccount == null)
        {
            ShowErrorMessage("Ошибка при поиске счета!");
            return;
        }

        if (!double.TryParse(TransactionAmountTextBox.Text, out double amount))
        {
            ShowErrorMessage("Введите корректную сумму!");
            return;
        }

        selectedAccount.Deposit(amount);

        Transaction newTransaction = new Transaction(
            selectedAccount.AccountNumber,
            Transaction.OperationType.Пополнение,
            DateTime.Now,
            amount);

        AddTransaction(newTransaction);

        FirstClientInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.GetAccountInfo()}";
        SecondClientInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.GetAccountInfo()}";

        ShowSuccessMessage("Операция выполнена успешно!");

        TransactionAmountTextBox.Clear();
    }

    private void OnWithdrawButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FirstClientSelector.SelectedItem == null)
            {
                ShowErrorMessage("Сначала выберите пользователя и карту!");
                return;
            }

            if (!double.TryParse(TransactionAmountTextBox.Text, out double amount) || amount <= 0)
            {
                ShowErrorMessage("Введите корректную сумму!");
                return;
            }

            string selectedUserName = FirstClientSelector.SelectedItem.ToString();
            var selectedAccount = _bankAccounts.FirstOrDefault(acc => acc.FullName == selectedUserName);

            if (selectedAccount == null)
            {
                ShowErrorMessage("Ошибка при поиске счета!");
                return;
            }

            if (!selectedAccount.Withdraw(amount))
            {
                ShowErrorMessage("Недостаточно средств на счете!");
                return;
            }

            Transaction newTransaction = new Transaction(
                selectedAccount.AccountNumber,
                Transaction.OperationType.Снятие,
                DateTime.Now,
                amount);

            AddTransaction(newTransaction);

            FirstClientInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.GetAccountInfo()}";
            SecondClientInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{selectedAccount.GetAccountInfo()}";
            
            ShowSuccessMessage("Операция выполнена успешно!");
            TransactionAmountTextBox.Clear();
        }
        catch (Exception ex)
        {
            ShowErrorMessage(ex.Message);
        }
    }

    private void OnTransferButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            if (FirstClientSelector.SelectedItem == null || SecondClientSelector.SelectedItem == null)
            {
                ShowErrorMessage("Выберите обоих пользователей!");
                return;
            }

            string senderName = FirstClientSelector.SelectedItem.ToString();
            string receiverName = SecondClientSelector.SelectedItem.ToString();
            
            if (senderName == receiverName)
            {
                ShowErrorMessage("Нельзя переводить самому себе!");
                return;
            }
            
            if (!double.TryParse(TransactionAmountTextBox.Text, out double amount) || amount <= 0)
            {
                ShowErrorMessage("Введите корректную положительную сумму!");
                return;
            }
            
            var senderAccount = _bankAccounts.FirstOrDefault(a => a.FullName == senderName);
            var receiverAccount = _bankAccounts.FirstOrDefault(a => a.FullName == receiverName);

            if (senderAccount == null || receiverAccount == null)
            {
                ShowErrorMessage("Один из счетов не найден!");
                return;
            }
            
            if (senderAccount.Status == BankAccount.AccountStatus.Закрыт || 
                receiverAccount.Status == BankAccount.AccountStatus.Закрыт)
            {
                ShowErrorMessage("Один из счетов закрыт!");
                return;
            }
            
            if (!senderAccount.Withdraw(amount))
            {
                ShowErrorMessage("Недостаточно средств для перевода!");
                return;
            }

            if (!receiverAccount.Deposit(amount))
            {
                senderAccount.Deposit(amount);
                ShowErrorMessage("Ошибка при зачислении средств!");
                return;
            }
            
            var transaction = new Transaction(
                senderAccount.AccountNumber,
                Transaction.OperationType.Перевод,
                DateTime.Now,
                amount,
                receiverAccount.AccountNumber,
                senderName,
                receiverName);

            _transfers.Add(transaction);
            
            FirstClientInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{senderAccount.GetAccountInfo()}";
            SecondClientInfoTextBox.Text = $"Выбранный пользователь: {Environment.NewLine}{receiverAccount.GetAccountInfo()}";

            ShowSuccessMessage("Перевод выполнен успешно!");
            TransactionAmountTextBox.Clear();
        }
        catch (Exception ex)
        {
            ShowErrorMessage($"Ошибка перевода: {ex.Message}");
        }
    }
    
    private void OnTransactionTypeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (FirstClientSelector == null || FirstClientSelector.SelectedItem == null || TransactionTypeSelector == null || TransactionTypeSelector.SelectedItem == null)
        {
            return;
        }

        if (FirstClientSelector.SelectedItem is string selectedUserName)
        {
            var bankAccount = _bankAccounts.FirstOrDefault(acc => acc.FullName == selectedUserName);
            if (bankAccount != null)
            {
                var Transaction = GetFilteredTransaction(bankAccount);
                UpdateTransactionOutput(bankAccount, TransactionLogTextBox, Transaction);
            }
        }
    }

    private List<Transaction> GetFilteredTransaction(BankAccount account)
    {
        if (TransactionTypeSelector == null || TransactionTypeSelector.SelectedItem == null)
        {
            return new List<Transaction>();
        }

        var selectedType = (TransactionTypeSelector.SelectedItem as ComboBoxItem)?.Content.ToString();

        var allTransaction = _deposits.Concat(_withdrawals).Concat(_transfers).ToList();

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
                _deposits.Add(transaction);
                break;
            case Transaction.OperationType.Снятие:
                _withdrawals.Add(transaction);
                break;
            case Transaction.OperationType.Перевод:
                _transfers.Add(transaction);
                break;
            default:
                throw new InvalidOperationException("Неизвестный тип транзакции.");
        }

        var bankAccount = _bankAccounts.FirstOrDefault(acc => acc.AccountNumber == transaction.AccountNumber);
        if (bankAccount != null)
        {
            UpdateTransactionOutput(bankAccount, TransactionLogTextBox, GetFilteredTransaction(bankAccount));
        }
    }

    private void UpdateTransactionOutput(BankAccount account, TextBox TransactionLogTextBox, List<Transaction> transactionList)
    {
        if (account == null || transactionList == null)
        {
            TransactionLogTextBox.Text = "Нет данных о транзакциях.";
            return;
        }

        if (transactionList.Count == 0)
        {
            TransactionLogTextBox.Text = "Нет совершенных транзакций.";
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
            TransactionLogTextBox.Text = output;
        });
    }

    private void OnFirstClientSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedClient(FirstClientSelector, FirstClientInfoTextBox);
    }

    private void OnSecondClientSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedClient(SecondClientSelector, SecondClientInfoTextBox);
    }

    private void UpdateSelectedClient(ComboBox ClientSelector, TextBox outputTextBox)
    {
        if (ClientSelector?.SelectedItem is string selectedUserName)
        {
            var selectedUser = _bankAccounts.FirstOrDefault(user => user.FullName == selectedUserName);

            if (selectedUser != null)
            {
                outputTextBox.Text = "Выбранный пользователь:" + Environment.NewLine +
                                      selectedUser.GetAccountInfo();
            }
            else
            {
                outputTextBox.Text = "Пользователь не найден.";
            }
        }
    }

    private void UpdateClientSelectors()
    {
        FirstClientSelector.Items.Clear();
        SecondClientSelector.Items.Clear();

        foreach (var user in _bankAccounts)
        {
            FirstClientSelector.Items.Add(user.FullName);
            SecondClientSelector.Items.Add(user.FullName);
        }

        if (_bankAccounts.Any())
        {
            FirstClientSelector.SelectedIndex = 0;
            SecondClientSelector.SelectedIndex = 0;
        }
    }
    
    private const string ClientsFile = "ClientsList.json";

    private void OnSaveClientsButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            string json = JsonConvert.SerializeObject(_bankAccounts, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(ClientsFile, json);
            ShowSuccessMessage("Данные успешно записаны в JSON!");
        }
        catch
        {
            ShowErrorMessage("Ошибка сохранения!");
        }
    }

    private void OnLoadClientsButtonClick(object sender, RoutedEventArgs e)
    {
        if (File.Exists(ClientsFile))
        {
            string json = File.ReadAllText(ClientsFile);
            _bankAccounts = JsonConvert.DeserializeObject<List<BankAccount>>(json);
            ShowSuccessMessage("Данные успешно считаны из JSON!");

            UpdateClientSelectors();
        }
        else
        {
            ShowErrorMessage("Ошибка при загрузке данных");
        }
    }
    
    private void ShowErrorMessage(string message)
    {
        MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void ShowSuccessMessage(string message)
    {
        MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}