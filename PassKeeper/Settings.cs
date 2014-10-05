using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassKeeper
{
    class PKSettings
    {
        private const string AUTOMATIC_SYNC_ONE_DRIVE_KEY = "onedrivesync";
        private const string OFFER_UPDATE_ONE_DRIVE_KEY = "offerupdate";
        //private const string 
        static IsolatedStorageSettings _appSettings = IsolatedStorageSettings.ApplicationSettings;

        static PKSettings _instance;
        public static PKSettings Instance
        {
            get { return _instance ?? (_instance = new PKSettings()); }
            set { _instance = value; }
        }        

        private PKSettings()
        {            
            if (!_appSettings.Contains(AUTOMATIC_SYNC_ONE_DRIVE_KEY))
                _appSettings.Add(AUTOMATIC_SYNC_ONE_DRIVE_KEY, false);
            if (!_appSettings.Contains(OFFER_UPDATE_ONE_DRIVE_KEY))
                _appSettings.Add(OFFER_UPDATE_ONE_DRIVE_KEY, false);
        }

        public void Save()
        {
            _appSettings.Save();
        }

        public bool AutomaticSyncOneDrive
        {
            get 
            {
                return (bool)_appSettings[AUTOMATIC_SYNC_ONE_DRIVE_KEY];                
            }
            set 
            {
                _appSettings[AUTOMATIC_SYNC_ONE_DRIVE_KEY] = value;
            }
        }

        public bool OfferSyncOneDriveAfterChanges
        {
            get
            {
                return (bool)_appSettings[OFFER_UPDATE_ONE_DRIVE_KEY];
            }
            set
            {
                _appSettings[OFFER_UPDATE_ONE_DRIVE_KEY] = value;
            }
        }
    }
}
