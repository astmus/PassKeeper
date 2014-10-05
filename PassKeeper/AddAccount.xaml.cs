using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PassKeeper
{
	public partial class AddAccount : UserControl
	{
		public bool IsEdit {get;set;}
		public AddAccount()
		{
			InitializeComponent();
		}
	}
}
