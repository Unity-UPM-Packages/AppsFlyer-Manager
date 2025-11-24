using System.Collections;
using System.Collections.Generic;
using TheLegends.Base.UnitySingleton;
using UnityEngine;
using AppsFlyerSDK;
using System;

#if UNITY_IOS
/*
1. In the Unity Editor, select Window > Package Manager to open the Package Manager.
2. Select the iOS 14 Advertising Support package from the list, then select the most recent verified version.
3. Click the Install or Update button.
*/

using UnityEngine.iOS;
using Unity.Advertisement.IosSupport;
using System.Runtime.InteropServices;
#endif


namespace TheLegends.Base.AppsFlyer
{
    public class AppsFlyerManager : PersistentMonoSingleton<AppsFlyerManager>, IAppsFlyerConversionData
    {
        public Action<string> OnInitComplete = null;
        public Action OnInitFailed = null;
        private GetConversionDataStatus conversionDataStatus;
        private readonly WaitForSeconds initDelay = new WaitForSeconds(0.1f);
        private readonly WaitForSeconds ATTDelay = new WaitForSeconds(0.5f);
        private float conversionTimeout = 5f;

        public void Init()
        {
            StartCoroutine(DoInit());
        }

        public IEnumerator DoInit(Action<string> onCompelte = null, Action onFailed = null, float conversionFetchingTimeout = 5f)
        {
            OnInitComplete = onCompelte;
            OnInitFailed = onFailed;
            conversionTimeout = conversionFetchingTimeout;

            yield return initDelay;

#if UNITY_IOS
            Version currentVersion = new Version("14.0");
            Version ios14 = new Version("14.5");
            try
            {
                if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
                    currentVersion = new Version(Device.systemVersion);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }

            var status = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();

            Debug.LogWarning("[ATTHelper] GetAuthorizationTrackingStatus: " + status + " iOS Version: " + currentVersion);

            yield return ATTDelay;

            if (status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED && currentVersion >= ios14)
            {
                //Change iOS in com.unity.ads.ios-support from  1.0.0 to 1.2.0 in "Packages/manifest.json" and "Packages/packages-lock.json"
                ATTrackingStatusBinding.RequestAuthorizationTracking((status) =>
                {
                    var authorizationTrackingStatus = ATTrackingStatusBinding.GetAuthorizationTrackingStatus();
                    Debug.LogWarning("[ATTHelper] RequestAuthorizationTracking: " + status + " " + authorizationTrackingStatus);

#if USE_FACEBOOK
                    if (authorizationTrackingStatus == ATTrackingStatusBinding.AuthorizationTrackingStatus.AUTHORIZED)
                        AudienceNetworkSettings.SetAdvertiserTrackingEnabled(true);
                    else
                        AudienceNetworkSettings.SetAdvertiserTrackingEnabled(false);
#endif
                });
            }

            float timeOut = 1.0f;
            while (timeOut > 0 && status == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED && currentVersion >= ios14)
            {
                timeOut -= Time.deltaTime;
                yield return null;
            }
#endif

            StartAppsFlyer();

            while (conversionDataStatus == GetConversionDataStatus.Fetching)
            {
                yield return null;
            }

            yield return initDelay;
        }

        private void StartAppsFlyer()
        {
            AppsFlyerSDK.AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);

            AppsFlyerSDK.AppsFlyer.setIsDebug(AppsFlyerSettings.Instance.isDebug);
            AppsFlyerSDK.AppsFlyer.initSDK(AppsFlyerSettings.Instance.devKey, AppsFlyerSettings.Instance.appID, AppsFlyerSettings.Instance.getConversionData ? this : null);

            AppsFlyerSDK.AppsFlyer.startSDK();

            StartHandleTimeout();

#if UNITY_EDITOR
            conversionDataStatus = GetConversionDataStatus.Success;
            StopHandleTimeout();
            OnInitComplete?.Invoke("Editor - No Conversion Data");
#elif UNITY_IOS || UNITY_ANDROID
            conversionDataStatus = AppsFlyerSettings.Instance.getConversionData ? GetConversionDataStatus.Fetching : GetConversionDataStatus.None;
#endif
        }

        protected void StartHandleTimeout()
        {
            Invoke(nameof(OnConversionDataTimeout), conversionTimeout);
        }

        protected void StopHandleTimeout()
        {
            CancelInvoke(nameof(OnConversionDataTimeout));
        }

        private void OnConversionDataTimeout()
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onConversionDataTimeout", $"Conversion data request timed out for {conversionTimeout}s.");
            conversionDataStatus = GetConversionDataStatus.Timeout;
            OnInitFailed?.Invoke();
        }

        // Mark AppsFlyer CallBacks
        public void onConversionDataSuccess(string conversionData)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("didReceiveConversionData", conversionData);
            Dictionary<string, object> conversionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here

            conversionDataStatus = GetConversionDataStatus.Success;
            StopHandleTimeout();
            OnInitComplete?.Invoke(conversionData);
        }

        public void onConversionDataFail(string error)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
            conversionDataStatus = GetConversionDataStatus.Fail;
            StopHandleTimeout();
            OnInitFailed?.Invoke();
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyerSDK.AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyerSDK.AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        public void LogEvent(string eventName, Dictionary<string, string> eventValues)
        {
#if USE_APPSFLYER
            AppsFlyerSDK.AppsFlyer.sendEvent(eventName, eventValues);
#endif
        }

        public void LogImpression(Dictionary<string, string> impressionData)
        {
            LogEvent("Ads_Impression", impressionData);
        }

        public void LogRevenue(string monetization, MediationNetwork mediation, string currency, double revenue, Dictionary<string, string> additionalParams)
        {
#if USE_APPSFLYER
            var logRevenue = new AFAdRevenueData(monetization, mediation, currency, revenue);
            AppsFlyerSDK.AppsFlyer.logAdRevenue(logRevenue, additionalParams);
#endif
        }

    }
}
