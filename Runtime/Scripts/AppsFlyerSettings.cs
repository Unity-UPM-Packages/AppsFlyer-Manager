using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLegends.Base.AppsFlyer
{
    [CreateAssetMenu(fileName = "AppsFlyerSettings", menuName = "DataAsset/AppsFlyerSettings")]
    public class AppsFlyerSettings : ScriptableObject
    {
        public const string ResDir = "Assets/TripSoft/AppsFlyer/Resources";
        public const string FileName = "AppsFlyerSettingsAsset";
        public const string FileExtension = ".asset";

        private static AppsFlyerSettings _instance;
        public static AppsFlyerSettings Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = Resources.Load<AppsFlyerSettings>(FileName);
                return _instance;
            }
        }

        public string devKey;
        public string appID;
        public bool getConversionData;
        public bool isDebug;
    }
}
