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

namespace PassKeeper
{
	public partial class PasswordPage : PhoneApplicationPage
	{
		public static object[] objs = new object[] { new { Sato = 1 }, new { Sato = 2 }, new { Sato = 3 } };

		TextBlock _lastSelectedLogin = new TextBlock();
		TextBlock _lastSelectedPasswprd =new TextBlock();
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
		}

		void AddAccount_Click(object sender, EventArgs e)
		{
			_currentAccountData = new AddAccount();			
			CustomMessageBox cmb = new CustomMessageBox();
			cmb.Caption = AppResources.MessageEnterAccountData;
			cmb.LeftButtonContent = "ok";
			cmb.RightButtonContent = "cancel";
			cmb.Content = _currentAccountData;
			cmb.Dismissed += cmb_Dismissed;
			cmb.Show();
			_dialogIsShow = true;
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
						Account a = new Account();
						a.Login = _currentAccountData.Login.Text;
						a.Name = _currentAccountData.Name.Text;
						a.Password = _currentAccountData.Password.Text;
						_accounts.Add(a);
						SaveAccounts();
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
				AddAccount_Click(null, null);
		}

		private void LoadAccounts()
		{
			IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

			if (isoStore.FileExists("accounts.dat"))
			{
				using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("accounts.dat", FileMode.Open, isoStore))
				{
					using (StreamReader reader = new StreamReader(isoStream))
					{
						_accounts = JsonConvert.DeserializeObject<ObservableCollection<Account>>(reader.ReadToEnd());
					}
				}
			}
			else
				_accounts = new ObservableCollection<Account>();			
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
		}
	}
}