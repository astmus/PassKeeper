using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PassKeeper
{
	public class Account : INotifyPropertyChanged, INotifyPropertyChanging
	{
		private string _name;
		public string Name
		{
			get { return _name; }
			set 
			{
				NotifyPropertyChanging();
				_name = value;
				NotifyPropertyChanged();
			}
		}

		private string _login;
		public string Login
		{
			get { return _login; }
			set 
			{
				NotifyPropertyChanging();
				_login = value;
				NotifyPropertyChanged();
			}
		}

		private string _password;
		public string Password
		{
			get { return _password; }
			set 
			{
				NotifyPropertyChanging();
				_password = value;
				NotifyPropertyChanged();
			}
		}
		
		// interface realization
		public event PropertyChangingEventHandler PropertyChanging;
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var propertyChangedCopy = PropertyChanged;
			if (propertyChangedCopy != null)
			{
				propertyChangedCopy(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected void NotifyPropertyChanging([CallerMemberName] string propertyName = "")
		{
			var propertyChangingCopy = PropertyChanging;
			if (propertyChangingCopy != null)
			{
				propertyChangingCopy(this, new PropertyChangingEventArgs(propertyName));
			}
		}
	
	}
}
