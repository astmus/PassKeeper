using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PassKeeper.Resources;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Threading;
using Microsoft.Phone.Reactive;

namespace PassKeeper
{
	public partial class PasswordPage : PhoneApplicationPage
	{
		TextBlock _lastSelectedLogin = new TextBlock();
		TextBlock _lastSelectedPasswprd = new TextBlock();
		SolidColorBrush _selectColor;
		SolidColorBrush _defaultColor;
		bool _dialogIsShow;
		ObservableCollection<Account> _accounts;
		AddAccount _currentAccountData;

		public PasswordPage()
		{
			InitializeComponent();
			BuildAppBar();
			_selectColor = (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
			_defaultColor = (SolidColorBrush)Application.Current.Resources["PhoneForegroundBrush"];
			LoadAccounts();
			PassHoslder.ItemsSource = _accounts;            
		}

		private void BuildAppBar()
		{
			ApplicationBar = new ApplicationBar();
			ApplicationBarIconButton button = new ApplicationBarIconButton(new Uri("/Images/add.png", UriKind.Relative));
			button.Text = AppResources.Add;
			button.Click += AddAccount_Click;
			ApplicationBar.Buttons.Add(button);

			ApplicationBarIconButton button2 = new ApplicationBarIconButton(new Uri("/Images/settings.png", UriKind.Relative));
			button2.Text = AppResources.Settings;
			button2.Click += ShowSettings_Click;
			ApplicationBar.Buttons.Add(button2);
		}

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (e.Content is Settings)
                (e.Content as Settings).PasswordsUpdated += PasswordPage_PasswordsUpdated;
            base.OnNavigatedFrom(e);
            
        }

        void PasswordPage_PasswordsUpdated()
        {
            LoadAccounts();
        }

		void ShowSettings_Click(object sender, EventArgs e)
		{
			NavigationService.Navigate(new Uri("/Settings.xaml",UriKind.Relative));
		}

		void AddAccount_Click(object sender, EventArgs e)
		{
			_currentAccountData = new AddAccount();
			ShowAddEditAccountDialog();
		}

		void cmb_Dismissed(object sender, DismissedEventArgs e)
		{
			switch (e.Result)
			{
				case CustomMessageBoxResult.LeftButton:
					{
						if (string.IsNullOrEmpty(_currentAccountData.Login.Text)) return;
						if (string.IsNullOrEmpty(_currentAccountData.Name.Text)) return;
						if (string.IsNullOrEmpty(_currentAccountData.Password.Text)) return;
						if (_currentAccountData.IsEdit)
						{
							Account editAccount = PassHoslder.SelectedItem as Account;
							editAccount.Name = _currentAccountData.Name.Text;
							editAccount.Login = _currentAccountData.Login.Text;
							editAccount.Password = _currentAccountData.Password.Text;
						}
						else
						{
							Account a = new Account();
							a.Login = _currentAccountData.Login.Text;
							a.Name = _currentAccountData.Name.Text;
							a.Password = _currentAccountData.Password.Text;
							_accounts.Add(a);
						}						
						SaveAccounts();
                        HandleOneDriveBehaviour();                               
                        
					}
					break;
				case CustomMessageBoxResult.RightButton:				
					break;
				default:
					break;
			}
			
			PassHoslder.SelectedItem = null;
			_dialogIsShow = false;
		}

        private void HandleOneDriveBehaviour()
        {
            if (PKSettings.Instance.OfferSyncOneDriveAfterChanges)
            {
                if (MessageBox.Show(AppResources.OfferSyncChanges, "", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    Scheduler.Dispatcher.Schedule(async () =>
                    {
                        await OneDrive.Instance.UploadPasswords(_accounts);
                    }, TimeSpan.FromMilliseconds(500));
            }
            else
                if (PKSettings.Instance.AutomaticSyncOneDrive)
                    Scheduler.Dispatcher.Schedule(async () =>
                    {
                        await OneDrive.Instance.UploadPasswords(_accounts);
                    }, TimeSpan.FromMilliseconds(500));
        }

		private void CopyLogin_Click(object sender, RoutedEventArgs e)
		{			
			PaintSelectedItem(sender as Button, true);
			this.ShowPopup(AppResources.MessageLoginWasCopied);
		}

		private void CopyPassword_Click_1(object sender, RoutedEventArgs e)
		{
			PaintSelectedItem(sender as Button, false);
			this.ShowPopup(AppResources.MessagePasswordWasCopien);
		}

		private void PaintSelectedItem(Button childButton, bool paintLogin)
		{
			Account data = childButton.DataContext as Account;
			ListBoxItem pressedItem = PassHoslder.ItemContainerGenerator.ContainerFromItem(data) as ListBoxItem;			
			TextBlock paintObj = (TextBlock)SearchVisualTree(pressedItem, paintLogin ? "loginTextBlock" : "passwordTextBlock");
			_lastSelectedLogin.Foreground = _defaultColor;
			_lastSelectedPasswprd.Foreground = _defaultColor;
			paintObj.Foreground = _selectColor;

			if (paintLogin)
			{
				_lastSelectedLogin = paintObj;
				Clipboard.SetText(data.Login);
			}
			else
			{
				_lastSelectedPasswprd = paintObj;
				Clipboard.SetText(data.Password);
			}
		}

		private FrameworkElement SearchVisualTree(DependencyObject targetElement, string name)
		{
			var count = VisualTreeHelper.GetChildrenCount(targetElement);
			if (count == 0)
				return null;

			for (int i = 0; i < count; i++)
			{
				FrameworkElement child = (FrameworkElement)VisualTreeHelper.GetChild(targetElement, i);
				if (child.Name == name)
					return child;
				else
				{
					FrameworkElement res = SearchVisualTree(child, name);
					if (res != null)
						return res;					
				}
			}
			return null;
		}

		private Popup _popup;
		TextBlock _popupTextBlock;
		// creates popup
		private Popup CreatePopup()
		{
			// text
			_popupTextBlock = new TextBlock();
			_popupTextBlock.Foreground = (Brush)this.Resources["PhoneForegroundBrush"];
			_popupTextBlock.FontSize = (double)this.Resources["PhoneFontSizeMedium"];
			_popupTextBlock.Margin = new Thickness(24, 32, 24, 12);			

			// grid wrapper
			Grid grid = new Grid();
			grid.Background = (Brush)this.Resources["PhoneAccentBrush"];
			grid.Children.Add(_popupTextBlock);
			grid.Width = this.ActualWidth;

			// popup
			Popup popup = new Popup();
			popup.Child = grid;

			return popup;
		}

		// hides popup
		private void HidePopup()
		{
			SystemTray.BackgroundColor = (Color)this.Resources["PhoneBackgroundColor"];
			this._popup.IsOpen = false;
		}

		// shows popup
		private async void ShowPopup(string message)
		{
			SystemTray.BackgroundColor = (Color)this.Resources["PhoneAccentColor"];

			if (this._popup == null)
				this._popup = this.CreatePopup();
			_popupTextBlock.Text = message;

			this._popup.IsOpen = true;

			await Task.Delay(1000);

			this.HidePopup();
		}

		private void PassHoslder_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!_dialogIsShow)
				EditAccount(PassHoslder.SelectedItem as Account);
		}

		private void EditAccount(Account account)
		{
			_currentAccountData = new AddAccount();
			_currentAccountData.Name.Text = account.Name;
			_currentAccountData.Login.Text = account.Login;
			_currentAccountData.Password.Text = account.Password;
			_currentAccountData.IsEdit = true;
			ShowAddEditAccountDialog();
		}

		private void ShowAddEditAccountDialog()
		{
			CustomMessageBox cmb = new CustomMessageBox();
			cmb.Caption = _currentAccountData.IsEdit ? AppResources.MessageEditAccount : AppResources.MessageEnterAccountData;
			cmb.LeftButtonContent = "ok";
			cmb.RightButtonContent = "cancel";
			cmb.Content = _currentAccountData;
			cmb.Dismissed += cmb_Dismissed;             
			cmb.Show();
            Scheduler.Dispatcher.Schedule(() =>
            {
                _currentAccountData.Name.Focus();
            }, TimeSpan.FromMilliseconds(200));
			_dialogIsShow = true;
		}

		private void LoadAccounts()
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            if (_accounts != null)
                _accounts.Clear();
            else
                _accounts = new ObservableCollection<Account>();

			if (isoStore.FileExists("accounts.dat"))
			{
				using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("accounts.dat", FileMode.Open, isoStore))
				{
					using (StreamReader reader = new StreamReader(isoStream))
					{
                        var loadedAccounts = JsonConvert.DeserializeObject<ObservableCollection<Account>>(reader.ReadToEnd());
                        foreach (Account a in loadedAccounts)
                            _accounts.Add(a);
					}
				}
			}			
		}

		private void SaveAccounts()
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
			using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("accounts.dat", FileMode.Create, isoStore))
			{
				using (StreamWriter writer = new StreamWriter(isoStream))
				{
					writer.WriteLine(JsonConvert.SerializeObject(_accounts));
					writer.Close();
				}
				
			}
		}

		private void DeleteItem_Click(object sender, RoutedEventArgs e)
		{
			Account r = (sender as MenuItem).DataContext as Account;
			_accounts.Remove(r);
			SaveAccounts();
            HandleOneDriveBehaviour();            
		}
	}
}