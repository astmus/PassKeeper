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
using System.IO.IsolatedStorage;

namespace PassKeeper
{
	public partial class MainPage : PhoneApplicationPage
	{
		// Constructor
		public MainPage()
		{
			InitializeComponent();

			Clipboard.SetText("asd");
			// Sample code to localize the ApplicationBar
			//BuildLocalizedApplicationBar();			
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			passwordField.Text = string.Empty;
		}

		private void SignIn_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(passwordField.Text))
			{
				MessageBox.Show(AppResources.MessageEnterPassword);
				passwordField.Focus();
				return;
			}
			IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
			if (!settings.Contains("mainPassword"))
			{
				settings.Add("mainPassword", passwordField.Text);
				settings.Save();
			}
			else
			{
				if ((string)settings["mainPassword"] != passwordField.Text)
				{
					MessageBox.Show(AppResources.MessageWrongPassword);
					return;
				}
			}
			NavigationService.Navigate(new Uri("/PasswordPage.xaml", UriKind.Relative));
		}


		// Sample code for building a localized ApplicationBar
		//private void BuildLocalizedApplicationBar()
		//{
		//    // Set the page's ApplicationBar to a new instance of ApplicationBar.
		//    ApplicationBar = new ApplicationBar();

		//    // Create a new button and set the text value to the localized string from AppResources.
		//    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
		//    appBarButton.Text = AppResources.AppBarButtonText;
		//    ApplicationBar.Buttons.Add(appBarButton);

		//    // Create a new menu item with the localized string from AppResources.
		//    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
		//    ApplicationBar.MenuItems.Add(appBarMenuItem);
		//}
	}
}