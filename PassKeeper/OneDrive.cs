using Microsoft.Live;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;

namespace PassKeeper
{
    public class OneDrive : OneDriveBase
    {
        private new static OneDrive _instance;
        public new static OneDrive Instance
        {
            get { return _instance ?? (_instance = new OneDrive()); }
        }

        protected OneDrive() : base()
        {

        }

        public override async Task UploadPasswords()
        {
            StartProgress();
            await base.UploadPasswords();
            StopProgress();
        }

        public override async Task<bool> DownloadPasswords()
        {
            StartProgress();
            bool result = await base.DownloadPasswords();
            StopProgress();
            return result;
        }

        public override async Task UploadPasswords(ObservableCollection<Account> accounts)
        {
            StartProgress();
            await base.UploadPasswords(accounts);
            StopProgress();
        }

        private void StartProgress()
        {
            if (SystemTray.ProgressIndicator == null)
                throw new Exception("there is not progress bar");
            SystemTray.ProgressIndicator.IsVisible = true;
            SystemTray.ProgressIndicator.IsIndeterminate = true;
        }

        private void StopProgress()
        {
            SystemTray.ProgressIndicator.IsVisible = false;
            SystemTray.ProgressIndicator.IsIndeterminate = false;
        }
    }

	public class OneDriveBase
	{
		static OneDriveBase _instance;
		const string _basePath = "me/skydrive";
		const string _passwordsFolder = "passwordskeeper";
        const string _passwordsFileName = "passwords.txt";
        bool _isConnected;
        string _pkFolderId;
        string _filePasswordsId;
        LiveAuthClient _authClient;
		LiveConnectClient _connectClient;

		public bool IsConnected
		{
			get { return _isConnected; }
			private set { _isConnected = value; }
		}

		protected OneDriveBase()
		{
			_authClient = new LiveAuthClient("000000004812A661");
		}

		public static OneDriveBase Instance
		{
			get { return _instance ?? (_instance = new OneDriveBase()); }			
		}

		public async Task<bool> Login()
		{
			IsConnected = false;
			try
			{				
				LiveLoginResult result = await _authClient.InitializeAsync(new string[] { "wl.signin", "wl.skydrive" });
				
				if (result.Status != LiveConnectSessionStatus.Connected)
					result = await _authClient.LoginAsync(new string[] { "wl.signin", "wl.skydrive" });

				if (result.Status == LiveConnectSessionStatus.Connected)
				{
					IsConnected = true;
					_connectClient = new LiveConnectClient(result.Session);
                    LiveOperationResult meResult = await _connectClient.GetAsync(_basePath + "/" + "files?filter=folders");
					_pkFolderId = GetPKFolderId(meResult.Result["data"]);                    
					if (_pkFolderId == null)
						_pkFolderId = await CreateFolder(_connectClient);
				}
			}
			catch (LiveAuthException ex)
			{
				IsConnected = false;
			}
			catch (LiveConnectException ex)
			{
				IsConnected = false;
			}
			return IsConnected;
		}
		public virtual async Task UploadPasswords()
		{
			if (!IsConnected)
				await Login();
			if (!IsConnected) return;

			IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
			if (isoStore.FileExists("accounts.dat"))
			{
				using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("accounts.dat", FileMode.Open, isoStore))
				{
					var res = await _connectClient.UploadAsync(_pkFolderId, "passwords.txt", isoStream, OverwriteOption.Overwrite);
                    _filePasswordsId = res.Result["id"] as string;                    
				}
			}
		}

        public virtual async Task<bool> DownloadPasswords()        
        {            
            if (!IsConnected)
                await Login();
            if (!IsConnected) return false;

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();
            string passwordFileId = await GetPasswordFileId();
            if (passwordFileId == null) return false;
            using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("accounts.dat", FileMode.OpenOrCreate, isoStore))
            {
                LiveDownloadOperationResult result = await _connectClient.DownloadAsync(passwordFileId + "/content");
                result.Stream.CopyTo(isoStream);
            }            
            return true;
        }
        
        public virtual async Task UploadPasswords(ObservableCollection<Account> accounts)
		{
			if (!IsConnected)
				await Login();
			if (!IsConnected) return;

			MemoryStream uploadStream = new MemoryStream();
			StreamWriter sw = new StreamWriter(uploadStream);
			sw.Write(JsonConvert.SerializeObject(accounts));
			sw.Flush();
			uploadStream.Seek(0, SeekOrigin.Begin);
			var res = await _connectClient.UploadAsync(_pkFolderId, "passwords.txt", uploadStream, OverwriteOption.Overwrite);
            _filePasswordsId = res.Result["id"] as String;
		}

        private async Task<string> GetPasswordFileId()
        {
            LiveOperationResult meResult = await _connectClient.GetAsync(_pkFolderId + "/files");
            dynamic dicts = meResult.Result["data"];
            foreach (dynamic item in dicts)
				if (item.name == _passwordsFileName)
					return item.id;
			return null;
        }

		private string GetPKFolderId(dynamic dicts)
		{
			foreach (dynamic item in dicts)
				if (item.name == _passwordsFolder)
					return item.id;
			return null;
		}

		private async Task<string> CreateFolder(LiveConnectClient client)
		{
			var folderData = new Dictionary<string, object>();
			folderData.Add("name", _passwordsFolder);
			LiveOperationResult operationResult = await client.PostAsync(_basePath, folderData);
			dynamic result = operationResult.Result;
			return result.id;
		}

	}
}
