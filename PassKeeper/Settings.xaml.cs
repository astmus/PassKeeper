using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Live;
using System.IO;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using PassKeeper.Resources;

namespace PassKeeper
{
	public partial class Settings : PhoneApplicationPage
	{
        public event Action PasswordsUpdated;
		public Settings()
		{
			InitializeComponent();
            DataContext = PKSettings.Instance;
		}

		private async void UploadAllPasswords(object sender, RoutedEventArgs e)
		{
			 await OneDrive.Instance.UploadPasswords();
		}

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            PKSettings.Instance.Save();
        }

        private void ApplyNewMasterPassword(object sender, RoutedEventArgs e)
        {
            if ((string)IsolatedStorageSettings.ApplicationSettings["mainPassword"] != oldPassword.Text)
                MessageBox.Show(AppResources.MessageWrongPassword);
            else
            {
                IsolatedStorageSettings.ApplicationSettings["mainPassword"] = newPassword.Text;
                IsolatedStorageSettings.ApplicationSettings.Save();
                oldPassword.Text = "";
                newPassword.Text = "";
            }
        }

        private async void GetAllPasswords(object sender, RoutedEventArgs e)
        {
            bool success = await OneDrive.Instance.DownloadPasswords();
            if (success && PasswordsUpdated != null)
                PasswordsUpdated();
        }
	}
}