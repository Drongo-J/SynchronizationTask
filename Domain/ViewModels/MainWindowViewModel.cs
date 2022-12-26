using SynchronizationTask.DataAccess.Entities;
using SynchronizationTask.Domain.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SynchronizationTask.Domain.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        public RelayCommand LoadDataCommmand { get; set; }
        public RelayCommand TransferCommand { get; set; }

        private string cardNumber = string.Empty;

        public string CardNumber
        {
            get { return cardNumber; }
            set { cardNumber = value; OnPropertyChanged(); }
        }

        private string customerFullname = "No Fullname";

        public string CustomerFullname
        {
            get { return customerFullname; }
            set { customerFullname = value; OnPropertyChanged(); }
        }

        private decimal balance = 0;

        public decimal Balance
        {
            get { return balance; }
            set { balance = value; OnPropertyChanged(); }
        }

        private string moneyToTransfer = string.Empty;

        public string MoneyToTransfer
        {
            get { return moneyToTransfer; }
            set { moneyToTransfer = value; OnPropertyChanged(); }
        }

        private string transferredMoney = string.Empty;

        public string TransferredMoney
        {
            get { return transferredMoney; }
            set { transferredMoney = value; OnPropertyChanged(); }
        }

        public Card Card { get; set; }

        public Mutex Mutex { get; set; }

        bool hasPressesTransferButton = false;

        public MainWindowViewModel()
        {
            var mainthread = Thread.CurrentThread;
            string mutexName = "MyMutex";
            Mutex = new Mutex(false, mutexName);
            var messageToShowInMsgBoxString = @" ===== CARD NUMBERS ===== 

            1111111111111111
            7148437819341278   
            2486778497864875   
            5837841237482375
            8344546578484875";
            MessageBox.Show(messageToShowInMsgBoxString);

            LoadDataCommmand = new RelayCommand((d) =>
            {
                if (CardNumber.Trim().Length == 16)
                {
                    Thread t = new Thread(() =>
                    {
                        Card = App.Database.Cards.FirstOrDefault(c => c.CardNumber == this.CardNumber);
                        if (Card == null)
                        {
                            MessageBox.Show("Card does not exits");
                            return;
                        }
                        var customerId = Card.CustomerId;
                        var customer = App.Database.Customers.FirstOrDefault(c => c.Id == customerId);
                        CustomerFullname = $"{customer.Name} {customer.Surname}";
                        Balance = Card.Balance;
                    }); t.Start();
                }
                else
                {
                    MessageBox.Show("Enter valid card number");
                }
            });

            TransferCommand = new RelayCommand((t) =>
            {
                Thread transferMoneyThread = new Thread(() =>
                {
                    if (hasPressesTransferButton == false)
                    {
                        if (MoneyToTransfer.Trim() == string.Empty)
                        {
                            if (Card == null)
                            {
                                MessageBox.Show("Enter card number first");
                            }
                            else
                            {
                                MessageBox.Show("Enter money amount to transfer");
                            }
                            return;
                        }

                        if (Balance - decimal.Parse(MoneyToTransfer) >= 0)
                        {
                            Mutex.WaitOne();
                            hasPressesTransferButton = true;
                            decimal value = decimal.Parse(MoneyToTransfer) / 12;
                            TransferredMoney = "0 AZN";
                            for (int i = 1; i < 12; i++)
                            {
                                Thread.Sleep(1000);
                                TransferredMoney = ((int)(value * i)).ToString() + " AZN";

                                if (i == 11)
                                {
                                    TransferredMoney = MoneyToTransfer + " AZN";
                                }

                            }
                            var m = decimal.Parse(MoneyToTransfer);
                            Balance -= m;

                            Card.Balance -= m;
                            App.Database.SaveChanges();
                            MessageBox.Show($"{MoneyToTransfer} AZN was transferred successfully!");
                            hasPressesTransferButton = false;
                            Mutex.ReleaseMutex();
                        }
                        else
                        {
                            MessageBox.Show("Not enough money in balance");
                        }
                    }
                });
                transferMoneyThread.Start();
            });
        }

        private static readonly Regex OnlyNumberRegex = new Regex("[0-9]+");
        public void IsAllowedInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private static bool IsTextAllowed(string text)
        {
            return OnlyNumberRegex.IsMatch(text);
        }
    }
}
