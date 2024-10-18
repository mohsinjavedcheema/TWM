using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Twm.Core.Annotations;
using Twm.Core.Classes;
using Twm.Core.Market;
using Twm.Model.Model;


namespace Twm.ViewModels.Accounts
{
    public class AccountsViewModel : DependencyObject, INotifyPropertyChanged
    {
        public ObservableCollection<AccountViewModel> Accounts

        {
            get { return (ObservableCollection<AccountViewModel>) GetValue(AccountsProperty); }
            set { SetValue(AccountsProperty, value); }
        }

        public static readonly DependencyProperty AccountsProperty =
            DependencyProperty.Register("Accounts", typeof(ObservableCollection<AccountViewModel>),
                typeof(AccountsViewModel),
                new UIPropertyMetadata(null));

        public ICollectionView AccountsView { get; set; }

        private AccountViewModel _selectedAccount;
        public AccountViewModel SelectedAccount
        {
            get { return _selectedAccount; }
            set
            {
                if (value != _selectedAccount)
                {
                    _selectedAccount = value;
                    OnPropertyChanged();
                }
            }
        }

        

        public AccountsViewModel()
        {
            Accounts = new ObservableCollection<AccountViewModel>();
            AccountsView = CollectionViewSource.GetDefaultView(Accounts);
            AccountsView.Filter += AccountsViewFilter;

            Session.Instance.Accounts.CollectionChanged += Accounts_CollectionChanged; 
        }


        private bool AccountsViewFilter(object item)
        {
            var account = item as AccountViewModel;
            if (account == null)
                return false;


            
            if (!account.IsActive)
            {
                return false;
            }

            return true;
        }


        private void Accounts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var account in e.NewItems.OfType<Account>())
                {
                    var accounTwm = new AccountViewModel(account);
                    accounTwm.PropertyChanged += AccounTwm_PropertyChanged;
                    Accounts.Add(accounTwm);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var idsForRemove = e.OldItems.OfType<Account>().Select(x => x.LocalId);
                var accountsForRemove = Accounts.Where(x => idsForRemove.Contains(x.Id)).ToList();
                foreach (var account in accountsForRemove)
                {
                    Accounts.Remove(account);
                }
            }
        }

        private void AccounTwm_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsActive")
            {
                AccountsView.Refresh();
                Session.Instance.ActiveAccounts.Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}